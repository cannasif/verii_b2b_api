using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Application.Communications.Services;
using Wms.Domain.Common;
using Wms.Domain.Entities.Communications;
using Wms.Infrastructure.Options;

namespace Wms.Infrastructure.Services.Communications;

public sealed class SmtpSettingsService : ISmtpSettingsService
{
    private readonly IRepository<SmtpSetting> _smtpSettings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;
    private readonly IMemoryCache _cache;
    private readonly IDataProtector _protector;
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<SmtpSettingsService> _logger;
    private const string CacheKey = "pragmatic_smtp_settings_runtime_v1";

    public SmtpSettingsService(
        IRepository<SmtpSetting> smtpSettings,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILocalizationService localizationService,
        IMemoryCache cache,
        IOptions<SmtpOptions> smtpOptions,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SmtpSettingsService> logger)
    {
        _smtpSettings = smtpSettings;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizationService = localizationService;
        _cache = cache;
        _smtpOptions = smtpOptions.Value;
        _protector = dataProtectionProvider.CreateProtector("pragmatic-smtp-settings-v1");
        _logger = logger;
    }

    public void InvalidateCache() => _cache.Remove(CacheKey);

    public async Task<ApiResponse<SmtpSettingsDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var entity = await EnsureSeededSettingsAsync(cancellationToken);
        return ApiResponse<SmtpSettingsDto>.SuccessResult(_mapper.Map<SmtpSettingsDto>(entity), _localizationService.GetLocalizedString("DataRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<SmtpSettingsDto>> UpdateAsync(UpdateSmtpSettingsDto dto, long userId, CancellationToken cancellationToken = default)
    {
        var entity = await _smtpSettings.Query(tracking: true)
            .OrderBy(x => x.Id)
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            entity = await _smtpSettings.Query(tracking: true, ignoreQueryFilters: true)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.IsDeleted = false;
                entity.DeletedDate = null;
                entity.DeletedBy = null;
            }
        }

        if (entity == null)
        {
            entity = new SmtpSetting
            {
                CreatedDate = DateTimeProvider.Now,
                CreatedBy = userId,
                IsDeleted = false
            };

            _mapper.Map(dto, entity);
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                entity.PasswordEncrypted = _protector.Protect(dto.Password);
            }

            entity.UpdatedDate = DateTimeProvider.Now;
            entity.UpdatedBy = userId;

            await _smtpSettings.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            InvalidateCache();

            return ApiResponse<SmtpSettingsDto>.SuccessResult(_mapper.Map<SmtpSettingsDto>(entity), _localizationService.GetLocalizedString("OperationSuccessful"));
        }

        _mapper.Map(dto, entity);
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            entity.PasswordEncrypted = _protector.Protect(dto.Password);
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        entity.UpdatedBy = userId;
        _smtpSettings.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        InvalidateCache();

        return ApiResponse<SmtpSettingsDto>.SuccessResult(_mapper.Map<SmtpSettingsDto>(entity), _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<SmtpSettingsRuntimeDto> GetRuntimeAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SmtpSettingsRuntimeDto? cached) && cached != null)
        {
            return cached;
        }

        var entity = await EnsureSeededSettingsAsync(cancellationToken);
        var decryptedPassword = string.Empty;

        if (!string.IsNullOrWhiteSpace(entity.PasswordEncrypted))
        {
            try
            {
                decryptedPassword = _protector.Unprotect(entity.PasswordEncrypted);
            }
            catch (CryptographicException ex)
            {
                decryptedPassword = _smtpOptions.Password ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(decryptedPassword))
                {
                    entity.PasswordEncrypted = _protector.Protect(decryptedPassword);
                    entity.UpdatedDate = DateTimeProvider.Now;
                    _smtpSettings.Update(entity);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    InvalidateCache();
                    _logger.LogInformation("SMTP password was re-protected using the current data protection key ring.");
                }
                else
                {
                    _logger.LogWarning(ex, "SMTP password could not be decrypted and no fallback SMTP password is configured.");
                }
            }
        }

        var runtime = new SmtpSettingsRuntimeDto
        {
            Host = entity.Host ?? string.Empty,
            Port = entity.Port,
            EnableSsl = entity.EnableSsl,
            Username = entity.Username ?? string.Empty,
            Password = decryptedPassword,
            FromEmail = entity.FromEmail ?? string.Empty,
            FromName = entity.FromName ?? string.Empty,
            Timeout = entity.Timeout
        };

        _cache.Set(CacheKey, runtime);
        return runtime;
    }

    private async Task<SmtpSetting> EnsureSeededSettingsAsync(CancellationToken cancellationToken)
    {
        var entity = await _smtpSettings.Query(tracking: true)
            .OrderBy(x => x.Id)
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity != null)
        {
            return entity;
        }

        entity = await _smtpSettings.Query(tracking: true, ignoreQueryFilters: true)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var host = _smtpOptions.Host;
        var username = _smtpOptions.Username;
        var password = _smtpOptions.Password;
        var fromEmail = !string.IsNullOrWhiteSpace(_smtpOptions.FromAddress) ? _smtpOptions.FromAddress : _smtpOptions.FromEmail;
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            fromEmail = "no-reply@localhost";
        }

        var fromName = _smtpOptions.FromName;
        var port = _smtpOptions.Port > 0 ? _smtpOptions.Port : 587;
        var timeout = _smtpOptions.Timeout > 0 ? _smtpOptions.Timeout : 30;
        var enableSsl = _smtpOptions.EnableSsl;

        if (entity == null)
        {
            entity = new SmtpSetting
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                Username = username,
                PasswordEncrypted = string.IsNullOrWhiteSpace(password) ? string.Empty : _protector.Protect(password),
                FromEmail = fromEmail,
                FromName = fromName,
                Timeout = timeout,
                IsDeleted = false,
                CreatedDate = DateTimeProvider.Now
            };

            await _smtpSettings.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.Host = host;
            entity.Port = port;
            entity.EnableSsl = enableSsl;
            entity.Username = username;
            entity.PasswordEncrypted = string.IsNullOrWhiteSpace(password) ? string.Empty : _protector.Protect(password);
            entity.FromEmail = fromEmail;
            entity.FromName = fromName;
            entity.Timeout = timeout;
            entity.IsDeleted = false;
            entity.DeletedDate = null;
            entity.DeletedBy = null;
            entity.UpdatedDate = DateTimeProvider.Now;
            _smtpSettings.Update(entity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        return entity;
    }
}
