using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wms.Domain.Entities.AccessControl;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
using Wms.Domain.Common;
using Wms.Domain.Entities.Identity;
using Wms.WebApi.Telemetry;

namespace Wms.Application.Identity.Services;

public sealed class AuthService : IAuthService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<UserSession> _userSessions;
    private readonly IRepository<PasswordResetRequest> _passwordResetRequests;
    private readonly IRepository<UserPermissionGroup> _userPermissionGroups;
    private readonly IRepository<UserAuthority> _userAuthorities;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IResetPasswordEmailJob _resetPasswordEmailJob;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> users,
        IRepository<UserSession> userSessions,
        IRepository<PasswordResetRequest> passwordResetRequests,
        IRepository<UserPermissionGroup> userPermissionGroups,
        IRepository<UserAuthority> userAuthorities,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILocalizationService localizationService,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IResetPasswordEmailJob resetPasswordEmailJob,
        IAuditLogWriter auditLogWriter,
        ILogger<AuthService> logger)
    {
        _users = users;
        _userSessions = userSessions;
        _passwordResetRequests = passwordResetRequests;
        _userPermissionGroups = userPermissionGroups;
        _userAuthorities = userAuthorities;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _localizationService = localizationService;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _resetPasswordEmailJob = resetPasswordEmailJob;
        _auditLogWriter = auditLogWriter;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _users.Query()
            .Include(x => x.RoleNavigation)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            var nf = _localizationService.GetLocalizedString("AuthUserNotFound");
            return ApiResponse<UserDto>.ErrorResult(nf, nf, 404);
        }

        return ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user), _localizationService.GetLocalizedString("AuthUserRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.Query()
            .Include(x => x.RoleNavigation)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<UserDto>>.SuccessResult(
            _mapper.Map<List<UserDto>>(users),
            _localizationService.GetLocalizedString("DataRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<IEnumerable<UserDto>>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.Query()
            .Include(x => x.RoleNavigation)
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        return ApiResponse<IEnumerable<UserDto>>.SuccessResult(
            _mapper.Map<List<UserDto>>(users),
            _localizationService.GetLocalizedString("ActiveUsersRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        var exists = await _users.Query()
            .Where(x => x.Username == registerDto.Username || x.Email == registerDto.Email)
            .AnyAsync(cancellationToken);

        if (exists)
        {
            var msg = _localizationService.GetLocalizedString("AuthUserAlreadyExists");
            return ApiResponse<UserDto>.ErrorResult(msg, msg, 400);
        }

        var user = _mapper.Map<User>(registerDto);
        user.PasswordHash = _passwordHasher.Hash(registerDto.Password);
        user.CreatedDate = DateTimeProvider.Now;
        user.IsDeleted = false;

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                await _resetPasswordEmailJob.SendUserCreatedEmailAsync(
                    user.Email,
                    user.Username,
                    registerDto.Password,
                    user.FirstName,
                    user.LastName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User created email could not be sent for user {UserId}.", user.Id);
            }
        }

        var persisted = await _users.Query()
            .Include(x => x.RoleNavigation)
            .Where(x => x.Id == user.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return ApiResponse<UserDto>.SuccessResult(
            _mapper.Map<UserDto>(persisted ?? user),
            _localizationService.GetLocalizedString("AuthUserRegisteredSuccessfully"));
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginRequest loginDto, CancellationToken cancellationToken = default)
    {
        var user = await _users.Query(tracking: true)
            .Include(x => x.RoleNavigation)
            .Where(x => x.Username == loginDto.Email || x.Email == loginDto.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || !_passwordHasher.Verify(loginDto.Password, user.PasswordHash))
        {
            var msg = _localizationService.GetLocalizedString("Error.User.InvalidCredentials");
            await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
            {
                EntityType = nameof(User),
                EntityId = user?.Id.ToString() ?? loginDto.Email.Trim(),
                ActionType = "Login",
                Result = RequestOutcome.ValidationFailed,
                Source = "AuthService.Login",
                NewValues = new { LoginIdentifier = loginDto.Email.Trim() },
                ChangedFields = new[] { "LoginIdentifier" },
                Reason = "InvalidCredentials"
            }, cancellationToken);
            return ApiResponse<string>.ErrorResult(msg, msg, 401);
        }

        user.LastLoginDate = DateTimeProvider.Now;
        var (permissions, isSystemAdmin) = await GetPermissionClaimsAsync(user.Id, user.RoleId, cancellationToken);
        var tokenResponse = _jwtService.GenerateToken(user, permissions, isSystemAdmin);
        if (!tokenResponse.Success || string.IsNullOrWhiteSpace(tokenResponse.Data))
        {
            return ApiResponse<string>.ErrorResult(
                _localizationService.GetLocalizedString("Error.User.LoginFailed"),
                tokenResponse.ExceptionMessage,
                tokenResponse.StatusCode == 0 ? 500 : tokenResponse.StatusCode);
        }

        var activeSessions = await _userSessions.Query(tracking: true)
            .Where(x => x.UserId == user.Id && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.RevokedAt = DateTimeProvider.Now;
            session.UpdatedDate = DateTimeProvider.Now;
        }

        var newSession = new UserSession
        {
            UserId = user.Id,
            SessionId = Guid.NewGuid(),
            CreatedAt = DateTimeProvider.Now,
            Token = ComputeSha256Hash(tokenResponse.Data!),
            CreatedDate = DateTimeProvider.Now,
            IsDeleted = false
        };

        await _userSessions.AddAsync(newSession, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(User),
            EntityId = user.Id.ToString(),
            ActionType = "Login",
            PerformedByUserId = user.Id,
            PerformedByUserEmail = user.Email,
            BranchCode = user.BranchCode,
            Result = RequestOutcome.Succeeded,
            Source = "AuthService.Login",
            NewValues = new
            {
                UserId = user.Id,
                user.Username,
                user.Email,
                SessionId = newSession.SessionId,
                LastLoginDate = user.LastLoginDate
            },
            ChangedFields = new[] { "LastLoginDate", "SessionId" }
        }, cancellationToken);

        return ApiResponse<string>.SuccessResult(tokenResponse.Data!, _localizationService.GetLocalizedString("Success.User.LoginSuccessful"));
    }

    public async Task<ApiResponse<string>> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.Query()
            .Where(x => x.Email == request.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (user != null)
        {
            var rawToken = Guid.NewGuid().ToString("N");
            var entity = new PasswordResetRequest
            {
                UserId = user.Id,
                TokenHash = ComputeSha256Hash(rawToken),
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _passwordResetRequests.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _resetPasswordEmailJob.SendPasswordResetEmailAsync(
                    user.Email,
                    string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                        ? user.Username
                        : string.Join(' ', new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))),
                    rawToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Password reset email could not be sent for user {UserId}.", user.Id);
            }
        }

        return ApiResponse<string>.SuccessResult(string.Empty, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = ComputeSha256Hash(request.Token);
        var now = DateTime.UtcNow;

        var resetRequest = await _passwordResetRequests.Query(tracking: true)
            .Include(x => x.User)
            .Where(x => x.TokenHash == tokenHash && x.UsedAt == null && x.ExpiresAt > now)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetRequest?.User == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<bool>.ErrorResult(msg, msg, 400);
        }

        resetRequest.UsedAt = now;
        resetRequest.UpdatedDate = now;
        resetRequest.User.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        resetRequest.User.UpdatedDate = now;

        await InvalidateUserSessionsAsync(resetRequest.User.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(resetRequest.User.Email))
        {
            try
            {
                await _resetPasswordEmailJob.SendPasswordResetCompletedEmailAsync(
                    resetRequest.User.Email,
                    string.IsNullOrWhiteSpace(resetRequest.User.FirstName) && string.IsNullOrWhiteSpace(resetRequest.User.LastName)
                        ? resetRequest.User.Username
                        : string.Join(' ', new[] { resetRequest.User.FirstName, resetRequest.User.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Password reset completion email could not be sent for user {UserId}.", resetRequest.User.Id);
            }
        }

        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.Query(tracking: true)
            .Include(x => x.RoleNavigation)
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            var nf = _localizationService.GetLocalizedString("AuthUserNotFound");
            await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
            {
                EntityType = nameof(User),
                EntityId = userId.ToString(),
                ActionType = "ChangePassword",
                PerformedByUserId = userId,
                Result = RequestOutcome.NotFound,
                Source = "AuthService.ChangePassword",
                Reason = "UserNotFound"
            }, cancellationToken);
            return ApiResponse<string>.ErrorResult(nf, nf, 404);
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            var msg = _localizationService.GetLocalizedString("Error.User.InvalidCredentials");
            await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
            {
                EntityType = nameof(User),
                EntityId = user.Id.ToString(),
                PerformedByUserId = user.Id,
                PerformedByUserEmail = user.Email,
                BranchCode = user.BranchCode,
                ActionType = "ChangePassword",
                Result = RequestOutcome.ValidationFailed,
                Source = "AuthService.ChangePassword",
                Reason = "CurrentPasswordMismatch"
            }, cancellationToken);
            return ApiResponse<string>.ErrorResult(msg, msg, 400);
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedDate = DateTimeProvider.Now;

        await InvalidateUserSessionsAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (permissions, isSystemAdmin) = await GetPermissionClaimsAsync(user.Id, user.RoleId, cancellationToken);
        var tokenResponse = _jwtService.GenerateToken(user, permissions, isSystemAdmin);
        if (!tokenResponse.Success || string.IsNullOrWhiteSpace(tokenResponse.Data))
        {
            return ApiResponse<string>.ErrorResult(
                _localizationService.GetLocalizedString("Error.User.LoginFailed"),
                tokenResponse.ExceptionMessage,
                tokenResponse.StatusCode == 0 ? 500 : tokenResponse.StatusCode);
        }

        var session = new UserSession
        {
            UserId = user.Id,
            SessionId = Guid.NewGuid(),
            CreatedAt = DateTimeProvider.Now,
            Token = ComputeSha256Hash(tokenResponse.Data!),
            CreatedDate = DateTimeProvider.Now,
            IsDeleted = false
        };

        await _userSessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(User),
            EntityId = user.Id.ToString(),
            ActionType = "ChangePassword",
            PerformedByUserId = user.Id,
            PerformedByUserEmail = user.Email,
            BranchCode = user.BranchCode,
            Result = RequestOutcome.Succeeded,
            Source = "AuthService.ChangePassword",
            OldValues = new
            {
                PreviousLastLoginDate = user.LastLoginDate
            },
            NewValues = new
            {
                UserId = user.Id,
                PasswordChanged = true,
                SessionId = session.SessionId
            },
            ChangedFields = new[] { "PasswordHash", "SessionId" }
        }, cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                await _resetPasswordEmailJob.SendPasswordChangedEmailAsync(
                    user.Email,
                    string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                        ? user.Username
                        : string.Join(' ', new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Password changed email could not be sent for user {UserId}.", user.Id);
            }
        }

        return ApiResponse<string>.SuccessResult(tokenResponse.Data!, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    private async Task InvalidateUserSessionsAsync(long userId, CancellationToken cancellationToken)
    {
        var sessions = await _userSessions.Query(tracking: true)
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAt = DateTimeProvider.Now;
            session.UpdatedDate = DateTimeProvider.Now;
        }
    }

    private async Task<(IReadOnlyCollection<string> Permissions, bool IsSystemAdmin)> GetPermissionClaimsAsync(long userId, long roleId, CancellationToken cancellationToken)
    {
        var roleTitle = await _userAuthorities.Query()
            .Where(x => x.Id == roleId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);

        var userGroupLinks = await _userPermissionGroups.Query()
            .Where(x => x.UserId == userId)
            .Include(x => x.PermissionGroup)
            .ThenInclude(x => x.GroupPermissions.Where(gp => !gp.IsDeleted))
            .ThenInclude(x => x.PermissionDefinition)
            .ToListAsync(cancellationToken);

        var isSystemAdmin = userGroupLinks.Any(x => x.PermissionGroup.IsSystemAdmin);
        if (!isSystemAdmin &&
            userGroupLinks.Count == 0 &&
            !string.IsNullOrWhiteSpace(roleTitle) &&
            roleTitle.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            isSystemAdmin = true;
        }

        var permissions = userGroupLinks
            .SelectMany(x => x.PermissionGroup.GroupPermissions)
            .Where(x => !x.IsDeleted && x.PermissionDefinition != null && !x.PermissionDefinition.IsDeleted && x.PermissionDefinition.IsActive)
            .Select(x => x.PermissionDefinition.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        return (permissions, isSystemAdmin);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();

        foreach (var value in bytes)
        {
            builder.Append(value.ToString("x2"));
        }

        return builder.ToString();
    }
}
