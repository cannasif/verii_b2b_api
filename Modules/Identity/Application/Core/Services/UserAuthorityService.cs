using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.Identity.Services;

public sealed class UserAuthorityService : IUserAuthorityService
{
    private static readonly HashSet<string> AllowedTitles = new(StringComparer.OrdinalIgnoreCase) { "user", "admin" };
    private readonly IRepository<UserAuthority> _authorities;
    private readonly IRepository<User> _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localization;
    public UserAuthorityService(IRepository<UserAuthority> authorities, IRepository<User> users, IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localization)
    { _authorities = authorities; _users = users; _unitOfWork = unitOfWork; _mapper = mapper; _localization = localization; }
    public async Task<ApiResponse<IEnumerable<UserAuthorityDto>>> GetAllAsync(CancellationToken cancellationToken = default)
        => ApiResponse<IEnumerable<UserAuthorityDto>>.SuccessResult(_mapper.Map<List<UserAuthorityDto>>(await _authorities.Query().Where(x => AllowedTitles.Contains(x.Title)).ToListAsync(cancellationToken)), _localization.GetLocalizedString("UserAuthorityRetrievedSuccessfully"));
    public async Task<ApiResponse<PagedResponse<UserAuthorityDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    { request ??= new(); var q = _authorities.Query().Where(x => AllowedTitles.Contains(x.Title)).ApplyFilters(request.Filters, request.FilterLogic).ApplySorting(request.SortBy ?? "Id", string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)); var total = await q.CountAsync(cancellationToken); var items = await q.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken); return ApiResponse<PagedResponse<UserAuthorityDto>>.SuccessResult(new(_mapper.Map<List<UserAuthorityDto>>(items), total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize), _localization.GetLocalizedString("UserAuthorityRetrievedSuccessfully")); }
    public async Task<ApiResponse<UserAuthorityDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    { var entity = await _authorities.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (entity == null || !AllowedTitles.Contains(entity.Title)) { var m = _localization.GetLocalizedString("UserAuthorityNotFound"); return ApiResponse<UserAuthorityDto>.ErrorResult(m,m,404);} return ApiResponse<UserAuthorityDto>.SuccessResult(_mapper.Map<UserAuthorityDto>(entity), _localization.GetLocalizedString("UserAuthorityRetrievedSuccessfully")); }
    public async Task<ApiResponse<UserAuthorityDto>> CreateAsync(CreateUserAuthorityDto createDto, CancellationToken cancellationToken = default)
    { if (!AllowedTitles.Contains(createDto.Title.Trim())) { var m = _localization.GetLocalizedString("OnlyUserAndAdminRolesAllowed"); return ApiResponse<UserAuthorityDto>.ErrorResult(m,m,400);} var entity = _mapper.Map<UserAuthority>(createDto); entity.Title = createDto.Title.Trim().ToLowerInvariant(); await _authorities.AddAsync(entity, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); return ApiResponse<UserAuthorityDto>.SuccessResult(_mapper.Map<UserAuthorityDto>(entity), _localization.GetLocalizedString("UserAuthorityCreatedSuccessfully")); }
    public async Task<ApiResponse<UserAuthorityDto>> UpdateAsync(long id, UpdateUserAuthorityDto updateDto, CancellationToken cancellationToken = default)
    { var entity = await _authorities.Query(tracking:true).FirstOrDefaultAsync(x => x.Id == id, cancellationToken); if (entity == null) { var m = _localization.GetLocalizedString("UserAuthorityNotFound"); return ApiResponse<UserAuthorityDto>.ErrorResult(m,m,404);} if (!AllowedTitles.Contains(updateDto.Title.Trim())) { var m = _localization.GetLocalizedString("OnlyUserAndAdminRolesAllowed"); return ApiResponse<UserAuthorityDto>.ErrorResult(m,m,400);} entity.Title = updateDto.Title.Trim().ToLowerInvariant(); _authorities.Update(entity); await _unitOfWork.SaveChangesAsync(cancellationToken); return ApiResponse<UserAuthorityDto>.SuccessResult(_mapper.Map<UserAuthorityDto>(entity), _localization.GetLocalizedString("UserAuthorityUpdatedSuccessfully")); }
    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    { if (!await _authorities.ExistsAsync(id,cancellationToken)) { var m = _localization.GetLocalizedString("UserAuthorityNotFound"); return ApiResponse<bool>.ErrorResult(m,m,404);} if (await _users.Query().AnyAsync(x => x.RoleId == id && !x.IsDeleted, cancellationToken)) { var m = _localization.GetLocalizedString("UserAuthorityCannotDeleteWhenUsersAssigned"); return ApiResponse<bool>.ErrorResult(m,m,400);} await _authorities.SoftDelete(id,cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); return ApiResponse<bool>.SuccessResult(true, _localization.GetLocalizedString("UserAuthorityDeletedSuccessfully")); }
}
