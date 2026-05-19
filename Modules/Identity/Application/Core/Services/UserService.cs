using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
using Wms.Domain.Common;
using Wms.Domain.Entities.AccessControl;
using Wms.Domain.Entities.B2B;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.Identity.Services;

public sealed class UserService : IUserService
{
    private readonly IRepository<User> _users; private readonly IRepository<UserPermissionGroup> _userPermissionGroups; private readonly IRepository<B2bCompany> _b2bCompanies; private readonly IRepository<B2bBuyer> _b2bBuyers; private readonly IUnitOfWork _uow; private readonly IMapper _mapper; private readonly ILocalizationService _loc; private readonly IPasswordHasher _hasher;
    public UserService(IRepository<User> users, IRepository<UserPermissionGroup> userPermissionGroups, IRepository<B2bCompany> b2bCompanies, IRepository<B2bBuyer> b2bBuyers, IUnitOfWork uow, IMapper mapper, ILocalizationService loc, IPasswordHasher hasher) { _users = users; _userPermissionGroups = userPermissionGroups; _b2bCompanies = b2bCompanies; _b2bBuyers = b2bBuyers; _uow = uow; _mapper = mapper; _loc = loc; _hasher = hasher; }
    public async Task<ApiResponse<PagedResponse<UserDto>>> GetAllUsersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new();

        var q = _users.Query()
            .ApplyFilters(request.Filters, request.FilterLogic);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var tokens = request.Search
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var token in tokens)
            {
                var currentToken = token;
                q = q.Where(x =>
                    x.Username.Contains(currentToken)
                    || x.Email.Contains(currentToken)
                    || (x.FirstName != null && x.FirstName.Contains(currentToken))
                    || (x.LastName != null && x.LastName.Contains(currentToken))
                    || (((x.FirstName ?? string.Empty) + " " + (x.LastName ?? string.Empty)).Trim().Contains(currentToken)));
            }
        }

        q = q.ApplySorting(request.SortBy ?? "Id", string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await q.CountAsync(cancellationToken);
        var items = await q.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtos = items.Select(Map).ToList();

        return ApiResponse<PagedResponse<UserDto>>.SuccessResult(
            new(dtos, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize),
            _loc.GetLocalizedString("UserRetrievedSuccessfully"));
    }
    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(long id, CancellationToken cancellationToken = default) { var user = await _users.Query().Include(x => x.RoleNavigation).FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (user == null || user.IsDeleted) { var m = _loc.GetLocalizedString("UserNotFound"); return ApiResponse<UserDto>.ErrorResult(m,m,404);} return ApiResponse<UserDto>.SuccessResult(Map(user), _loc.GetLocalizedString("UserRetrievedSuccessfully")); }
    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default) { if (await _users.Query().AnyAsync(x => x.Username == dto.Username || x.Email == dto.Email, cancellationToken)) { var m = _loc.GetLocalizedString("UserAlreadyExists"); return ApiResponse<UserDto>.ErrorResult(m,m,400);} var buyerValidation = await ValidateBuyerProfileRequestAsync(dto.AccountType, dto.B2bCompanyId, cancellationToken); if (!buyerValidation.Success) return ApiResponse<UserDto>.ErrorResult(buyerValidation.Message, buyerValidation.ExceptionMessage, buyerValidation.StatusCode); var user = _mapper.Map<User>(dto); user.PasswordHash = string.IsNullOrWhiteSpace(dto.Password) ? _hasher.Hash("123456") : _hasher.Hash(dto.Password); user.IsActive = dto.IsActive ?? true; user.CreatedDate = DateTimeProvider.Now; await _users.AddAsync(user,cancellationToken); await _uow.SaveChangesAsync(cancellationToken); await ReplacePermissionGroupsAsync(user.Id, dto.PermissionGroupIds, cancellationToken); var buyerResult = await UpsertBuyerProfileAsync(user, dto.AccountType, dto.B2bCompanyId, dto.B2bBuyerRoleCode, dto.B2bOrderLimit, dto.B2bRequiresApproval, cancellationToken); if (!buyerResult.Success) return ApiResponse<UserDto>.ErrorResult(buyerResult.Message, buyerResult.ExceptionMessage, buyerResult.StatusCode); return ApiResponse<UserDto>.SuccessResult(Map(user), _loc.GetLocalizedString("UserCreatedSuccessfully")); }
    public async Task<ApiResponse<UserDto>> UpdateUserAsync(long id, UpdateUserDto dto, CancellationToken cancellationToken = default) { var user = await _users.Query(tracking:true).Include(x => x.RoleNavigation).FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (user == null || user.IsDeleted) { var m = _loc.GetLocalizedString("UserNotFound"); return ApiResponse<UserDto>.ErrorResult(m,m,404);} var buyerValidation = await ValidateBuyerProfileRequestAsync(dto.AccountType, dto.B2bCompanyId, cancellationToken); if (!buyerValidation.Success) return ApiResponse<UserDto>.ErrorResult(buyerValidation.Message, buyerValidation.ExceptionMessage, buyerValidation.StatusCode); _mapper.Map(dto,user); user.UpdatedDate = DateTimeProvider.Now; _users.Update(user); await _uow.SaveChangesAsync(cancellationToken); await ReplacePermissionGroupsAsync(user.Id, dto.PermissionGroupIds, cancellationToken); var buyerResult = await UpsertBuyerProfileAsync(user, dto.AccountType, dto.B2bCompanyId, dto.B2bBuyerRoleCode, dto.B2bOrderLimit, dto.B2bRequiresApproval, cancellationToken); if (!buyerResult.Success) return ApiResponse<UserDto>.ErrorResult(buyerResult.Message, buyerResult.ExceptionMessage, buyerResult.StatusCode); return ApiResponse<UserDto>.SuccessResult(Map(user), _loc.GetLocalizedString("UserUpdatedSuccessfully")); }
    public async Task<ApiResponse<object>> DeleteUserAsync(long id, CancellationToken cancellationToken = default) { var user = await _users.Query(tracking:true).FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (user == null || user.IsDeleted) { var m = _loc.GetLocalizedString("UserNotFound"); return ApiResponse<object>.ErrorResult(m,m,404);} user.IsDeleted = true; user.DeletedDate = DateTimeProvider.Now; _users.Update(user); await _uow.SaveChangesAsync(cancellationToken); return ApiResponse<object>.SuccessResult(new { id }, _loc.GetLocalizedString("UserDeletedSuccessfully")); }
    private async Task ReplacePermissionGroupsAsync(long userId, List<long>? ids, CancellationToken cancellationToken) { if (ids == null) return; var existing = await _userPermissionGroups.Query(tracking:true).Where(x => x.UserId == userId && !x.IsDeleted).ToListAsync(cancellationToken); foreach (var item in existing) item.MarkAsDeleted(); foreach (var id in ids.Distinct()) await _userPermissionGroups.AddAsync(new UserPermissionGroup { UserId = userId, PermissionGroupId = id, CreatedDate = DateTimeProvider.Now }, cancellationToken); await _uow.SaveChangesAsync(cancellationToken); }
    private async Task<ApiResponse<bool>> ValidateBuyerProfileRequestAsync(string? accountType, long? companyId, CancellationToken cancellationToken)
    {
        if (!string.Equals(accountType, "Buyer", StringComparison.OrdinalIgnoreCase) && !string.Equals(accountType, "B2BBuyer", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<bool>.SuccessResult(true, "No B2B buyer profile requested");
        }

        if (!companyId.HasValue || companyId.Value <= 0)
        {
            return ApiResponse<bool>.ErrorResult("B2B buyer kullanıcısı için şirket seçimi zorunludur.", statusCode: 400);
        }

        var companyExists = await _b2bCompanies.Query().AnyAsync(x => !x.IsDeleted && x.Id == companyId.Value, cancellationToken);
        return companyExists
            ? ApiResponse<bool>.SuccessResult(true, "B2B company validated")
            : ApiResponse<bool>.ErrorResult("B2B şirket hesabı bulunamadı.", statusCode: 404);
    }
    private async Task<ApiResponse<bool>> UpsertBuyerProfileAsync(User user, string? accountType, long? companyId, string? buyerRoleCode, decimal? orderLimit, bool? requiresApproval, CancellationToken cancellationToken)
    {
        if (!string.Equals(accountType, "Buyer", StringComparison.OrdinalIgnoreCase) && !string.Equals(accountType, "B2BBuyer", StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<bool>.SuccessResult(true, "No B2B buyer profile requested");
        }

        if (!companyId.HasValue || companyId.Value <= 0)
        {
            return ApiResponse<bool>.ErrorResult("B2B buyer kullanıcısı için şirket seçimi zorunludur.", statusCode: 400);
        }

        var companyExists = await _b2bCompanies.Query().AnyAsync(x => !x.IsDeleted && x.Id == companyId.Value, cancellationToken);
        if (!companyExists)
        {
            return ApiResponse<bool>.ErrorResult("B2B şirket hesabı bulunamadı.", statusCode: 404);
        }

        var buyer = await _b2bBuyers.Query(tracking: true).FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == user.Id, cancellationToken);
        if (buyer is null)
        {
            buyer = new B2bBuyer
            {
                UserId = user.Id,
                CreatedDate = DateTimeProvider.Now
            };
            await _b2bBuyers.AddAsync(buyer, cancellationToken);
        }

        buyer.CompanyId = companyId.Value;
        buyer.Email = user.Email.Trim().ToLowerInvariant();
        buyer.FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
        buyer.RoleCode = string.IsNullOrWhiteSpace(buyerRoleCode) ? "Buyer" : buyerRoleCode.Trim();
        buyer.OrderLimit = orderLimit;
        buyer.RequiresApproval = requiresApproval ?? buyer.RequiresApproval;
        buyer.IsActive = user.IsActive;
        buyer.UpdatedDate = DateTimeProvider.Now;
        await _uow.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, "B2B buyer profile upserted");
    }
    private static UserDto Map(User user) => new() { Id = user.Id, Username = user.Username, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber, RoleId = user.RoleId, Role = user.RoleNavigation?.Title ?? string.Empty, IsEmailConfirmed = user.IsEmailConfirmed, IsActive = user.IsActive, LastLoginDate = user.LastLoginDate, FullName = user.FullName, CreatedDate = user.CreatedDate, UpdatedDate = user.UpdatedDate, DeletedDate = user.DeletedDate, IsDeleted = user.IsDeleted, CreatedBy = user.CreatedBy, UpdatedBy = user.UpdatedBy, DeletedBy = user.DeletedBy };
}
