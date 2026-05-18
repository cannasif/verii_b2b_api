using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Customer.Dtos;
using Wms.Domain.Common;
using CustomerEntity = Wms.Domain.Entities.Customer.Customer;

namespace Wms.Application.Customer.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IRepository<CustomerEntity> _customers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public CustomerService(
        IRepository<CustomerEntity> customers,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _customers = customers;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<CustomerDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await BuildQuery().ToListAsync(cancellationToken);
        return ApiResponse<IEnumerable<CustomerDto>>.SuccessResult(_mapper.Map<List<CustomerDto>>(items), _localizationService.GetLocalizedString("CustomerRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<PagedResponse<CustomerDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = BuildQuery()
            .ApplySearch(request.Search,
                nameof(CustomerEntity.CustomerCode),
                nameof(CustomerEntity.CustomerName),
                nameof(CustomerEntity.TaxOffice),
                nameof(CustomerEntity.TaxNumber),
                nameof(CustomerEntity.TcknNumber),
                nameof(CustomerEntity.SalesRepCode),
                nameof(CustomerEntity.GroupCode),
                nameof(CustomerEntity.Email),
                nameof(CustomerEntity.Website),
                nameof(CustomerEntity.Phone1),
                nameof(CustomerEntity.Phone2),
                nameof(CustomerEntity.Address),
                nameof(CustomerEntity.City),
                nameof(CustomerEntity.District),
                nameof(CustomerEntity.CountryCode))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(CustomerEntity.Id), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtoItems = _mapper.Map<List<CustomerDto>>(items);
        var page = new PagedResponse<CustomerDto>(dtoItems, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize);
        return ApiResponse<PagedResponse<CustomerDto>>.SuccessResult(page, _localizationService.GetLocalizedString("CustomerRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<CustomerDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _customers.Query().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("CustomerNotFound");
            return ApiResponse<CustomerDto>.ErrorResult(message, message, 404);
        }

        return ApiResponse<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(entity), _localizationService.GetLocalizedString("CustomerRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto createDto, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(createDto.CustomerCode);
        var exists = await _customers.Query().AnyAsync(x => x.CustomerCode == normalizedCode && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            var message = _localizationService.GetLocalizedString("CustomerAlreadyExists");
            return ApiResponse<CustomerDto>.ErrorResult(message, message, 400);
        }

        var entity = _mapper.Map<CustomerEntity>(createDto) ?? new CustomerEntity();
        entity.CustomerCode = normalizedCode;
        entity.BranchCode = NormalizeBranchCode(createDto.BranchCode);
        entity.CreatedDate = DateTimeProvider.Now;
        entity.LastSyncDate = DateTimeProvider.Now;
        await _customers.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(entity), _localizationService.GetLocalizedString("CustomerCreatedSuccessfully"));
    }

    public async Task<ApiResponse<CustomerDto>> UpdateAsync(long id, UpdateCustomerDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _customers.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("CustomerNotFound");
            return ApiResponse<CustomerDto>.ErrorResult(message, message, 404);
        }

        if (!string.IsNullOrWhiteSpace(updateDto.CustomerCode))
        {
            var normalizedCode = NormalizeCode(updateDto.CustomerCode);
            var duplicate = await _customers.Query().AnyAsync(x => x.Id != id && x.CustomerCode == normalizedCode && !x.IsDeleted, cancellationToken);
            if (duplicate)
            {
                var message = _localizationService.GetLocalizedString("CustomerAlreadyExists");
                return ApiResponse<CustomerDto>.ErrorResult(message, message, 400);
            }
        }

        _mapper.Map(updateDto, entity);
        if (!string.IsNullOrWhiteSpace(updateDto.CustomerCode))
        {
            entity.CustomerCode = NormalizeCode(updateDto.CustomerCode);
        }

        if (!string.IsNullOrWhiteSpace(updateDto.BranchCode))
        {
            entity.BranchCode = NormalizeBranchCode(updateDto.BranchCode);
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _customers.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDto>.SuccessResult(_mapper.Map<CustomerDto>(entity), _localizationService.GetLocalizedString("CustomerUpdatedSuccessfully"));
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var exists = await _customers.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            var message = _localizationService.GetLocalizedString("CustomerNotFound");
            return ApiResponse<bool>.ErrorResult(message, message, 404);
        }

        await _customers.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("CustomerDeletedSuccessfully"));
    }

    public async Task<ApiResponse<int>> CustomerSyncAsync(IEnumerable<SyncCustomerDto> customers, CancellationToken cancellationToken = default)
    {
        var input = customers?
            .Where(x => !string.IsNullOrWhiteSpace(x.CustomerCode) && !string.IsNullOrWhiteSpace(x.CustomerName))
            .ToList() ?? new List<SyncCustomerDto>();

        if (input.Count == 0)
        {
            return ApiResponse<int>.SuccessResult(0, _localizationService.GetLocalizedString("CustomerSyncCompletedSuccessfully"));
        }

        var now = DateTimeProvider.Now;
        var codes = input.Select(x => NormalizeCode(x.CustomerCode)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existing = await _customers.Query(tracking: true)
            .Where(x => codes.Contains(x.CustomerCode))
            .ToListAsync(cancellationToken);
        var existingByCode = existing.ToDictionary(x => NormalizeCode(x.CustomerCode), StringComparer.OrdinalIgnoreCase);

        var insertedCount = 0;
        foreach (var item in input)
        {
            var code = NormalizeCode(item.CustomerCode);
            if (existingByCode.TryGetValue(code, out var entity))
            {
                MapSync(entity, item, now);
                entity.IsDeleted = false;
                entity.DeletedDate = null;
                entity.DeletedBy = null;
                entity.UpdatedDate = now;
                _customers.Update(entity);
                continue;
            }

            var newEntity = _mapper.Map<CustomerEntity>(item) ?? new CustomerEntity();
            newEntity.CustomerCode = code;
            newEntity.BranchCode = NormalizeBranchCode(item.BranchCode);
            newEntity.CreatedDate = now;
            newEntity.UpdatedDate = now;
            newEntity.LastSyncDate = now;
            newEntity.IsErpIntegrated = item.IsErpIntegrated;
            await _customers.AddAsync(newEntity, cancellationToken);
            insertedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<int>.SuccessResult(insertedCount, _localizationService.GetLocalizedString("CustomerSyncCompletedSuccessfully"));
    }

    private IQueryable<CustomerEntity> BuildQuery()
    {
        return _customers.Query().Where(x => !x.IsDeleted);
    }

    private static void MapSync(CustomerEntity entity, SyncCustomerDto item, DateTime now)
    {
        entity.CustomerName = item.CustomerName.Trim();
        entity.TaxOffice = item.TaxOffice?.Trim();
        entity.TaxNumber = item.TaxNumber?.Trim();
        entity.TcknNumber = item.TcknNumber?.Trim();
        entity.SalesRepCode = item.SalesRepCode?.Trim();
        entity.GroupCode = item.GroupCode?.Trim();
        entity.CreditLimit = item.CreditLimit;
        entity.BranchCode = NormalizeBranchCode(item.BranchCode);
        entity.BusinessUnitCode = item.BusinessUnitCode;
        entity.Email = item.Email?.Trim();
        entity.Website = item.Website?.Trim();
        entity.Phone1 = item.Phone1?.Trim();
        entity.Phone2 = item.Phone2?.Trim();
        entity.Address = item.Address?.Trim();
        entity.City = item.City?.Trim();
        entity.District = item.District?.Trim();
        entity.CountryCode = item.CountryCode?.Trim();
        entity.IsErpIntegrated = item.IsErpIntegrated;
        entity.ErpIntegrationNumber = item.ErpIntegrationNumber?.Trim();
        entity.LastSyncDate = now;
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    private static string NormalizeBranchCode(string? branchCode) => string.IsNullOrWhiteSpace(branchCode) ? "0" : branchCode.Trim();
}
