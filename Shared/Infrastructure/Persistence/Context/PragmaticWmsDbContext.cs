using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Wms.Domain.Entities.AccessControl;
using Wms.Domain.Entities.B2B;
using Wms.Domain.Entities.Communications;
using Wms.Domain.Entities.Common;
using Wms.Domain.Entities.Customer;
using Wms.Domain.Entities.Identity;
using Wms.Domain.Entities.Stock;
using Wms.Domain.Entities.Warehouse;
using Wms.Domain.Entities.YapKod;

namespace Wms.Infrastructure.Persistence.Context;

public sealed class WmsDbContext : DbContext
{
    public WmsDbContext(DbContextOptions<WmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAuthority> UserAuthorities => Set<UserAuthority>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserDetail> UserDetails => Set<UserDetail>();
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<PermissionGroupPermission> PermissionGroupPermissions => Set<PermissionGroupPermission>();
    public DbSet<UserPermissionGroup> UserPermissionGroups => Set<UserPermissionGroup>();
    public DbSet<WmsScopePolicy> WmsScopePolicies => Set<WmsScopePolicy>();
    public DbSet<UserWmsScopePolicy> UserWmsScopePolicies => Set<UserWmsScopePolicy>();
    public DbSet<SmtpSetting> SmtpSettings => Set<SmtpSetting>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockDetail> StockDetails => Set<StockDetail>();
    public DbSet<StockImage> StockImages => Set<StockImage>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<YapKod> YapKodlar => Set<YapKod>();
    public DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();
    public DbSet<CatalogVariant> CatalogVariants => Set<CatalogVariant>();
    public DbSet<CatalogCategory> CatalogCategories => Set<CatalogCategory>();
    public DbSet<CatalogProductCategory> CatalogProductCategories => Set<CatalogProductCategory>();
    public DbSet<CatalogAttributeDefinition> CatalogAttributeDefinitions => Set<CatalogAttributeDefinition>();
    public DbSet<CatalogProductAttribute> CatalogProductAttributes => Set<CatalogProductAttribute>();
    public DbSet<CatalogProductMedia> CatalogProductMedia => Set<CatalogProductMedia>();
    public DbSet<CatalogProductDocument> CatalogProductDocuments => Set<CatalogProductDocument>();
    public DbSet<CatalogProductFavorite> CatalogProductFavorites => Set<CatalogProductFavorite>();
    public DbSet<CatalogCategoryFavorite> CatalogCategoryFavorites => Set<CatalogCategoryFavorite>();
    public DbSet<CustomerProductAlias> CustomerProductAliases => Set<CustomerProductAlias>();
    public DbSet<B2bCart> B2bCarts => Set<B2bCart>();
    public DbSet<B2bCartLine> B2bCartLines => Set<B2bCartLine>();
    public DbSet<B2bOrder> B2bOrders => Set<B2bOrder>();
    public DbSet<B2bOrderLine> B2bOrderLines => Set<B2bOrderLine>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<PaymentProviderInquiryLog> PaymentProviderInquiryLogs => Set<PaymentProviderInquiryLog>();
    public DbSet<PaymentProviderOperation> PaymentProviderOperations => Set<PaymentProviderOperation>();
    public DbSet<PaymentMethodRule> PaymentMethodRules => Set<PaymentMethodRule>();
    public DbSet<CustomerPriceList> CustomerPriceLists => Set<CustomerPriceList>();
    public DbSet<CustomerPriceListItem> CustomerPriceListItems => Set<CustomerPriceListItem>();
    public DbSet<InventorySnapshot> InventorySnapshots => Set<InventorySnapshot>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<QuoteRequestLine> QuoteRequestLines => Set<QuoteRequestLine>();
    public DbSet<B2bIntegrationEvent> B2bIntegrationEvents => Set<B2bIntegrationEvent>();
    public DbSet<B2bCompany> B2bCompanies => Set<B2bCompany>();
    public DbSet<B2bBuyer> B2bBuyers => Set<B2bBuyer>();
    public DbSet<B2bCompanyAddress> B2bCompanyAddresses => Set<B2bCompanyAddress>();
    public DbSet<CatalogVisibilityRule> CatalogVisibilityRules => Set<CatalogVisibilityRule>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListLine> ShoppingListLines => Set<ShoppingListLine>();
    public DbSet<PurchaseApprovalRule> PurchaseApprovalRules => Set<PurchaseApprovalRule>();
    public DbSet<MarketplaceChannel> MarketplaceChannels => Set<MarketplaceChannel>();
    public DbSet<MarketplaceListing> MarketplaceListings => Set<MarketplaceListing>();
    public DbSet<MarketplaceSyncEvent> MarketplaceSyncEvents => Set<MarketplaceSyncEvent>();
    public DbSet<WmsAuditLog> WmsAuditLogs => Set<WmsAuditLog>();
    public DbSet<WmsIntegrationLog> WmsIntegrationLogs => Set<WmsIntegrationLog>();
    public DbSet<JobFailureLog> JobFailureLogs => Set<JobFailureLog>();
    public DbSet<JobExecutionLog> JobExecutionLogs => Set<JobExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WmsDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyBranchCodes();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyBranchCodes();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBranchCodes();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyBranchCodes();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyBranchCodes()
    {
        var entries = ChangeTracker.Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Where(x => x.Entity is IBranchScopedEntity)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseHeaderEntity headerEntity)
            {
                headerEntity.BranchCode = NormalizeBranchCode(headerEntity.BranchCode);
                continue;
            }

            var scopedEntity = (IBranchScopedEntity)entry.Entity;
            var resolvedBranchCode = ResolveBranchCodeFromHeader(entry) ?? scopedEntity.BranchCode;
            scopedEntity.BranchCode = NormalizeBranchCode(resolvedBranchCode);
        }
    }

    private string? ResolveBranchCodeFromHeader(EntityEntry entry)
    {
        var headerNavigation = entry.Metadata.GetNavigations()
            .FirstOrDefault(x => typeof(BaseHeaderEntity).IsAssignableFrom(x.TargetEntityType.ClrType));

        if (headerNavigation != null)
        {
            var navigationEntry = entry.Navigation(headerNavigation.Name);
            if (navigationEntry.CurrentValue is BaseHeaderEntity trackedHeader)
            {
                return trackedHeader.BranchCode;
            }
        }

        var headerIdProperty = entry.Metadata.FindProperty("HeaderId");
        if (headerIdProperty?.ClrType != typeof(long) && headerIdProperty?.ClrType != typeof(long?))
        {
            return null;
        }

        var headerIdValue = entry.Property("HeaderId").CurrentValue;
        if (headerIdValue == null)
        {
            return null;
        }

        var headerEntityType = headerNavigation?.TargetEntityType.ClrType;
        if (headerEntityType == null)
        {
            return null;
        }

        var trackedHeaderEntry = ChangeTracker.Entries()
            .FirstOrDefault(x => x.Metadata.ClrType == headerEntityType && x.Property("Id").CurrentValue?.Equals(headerIdValue) == true);

        if (trackedHeaderEntry?.Entity is BaseHeaderEntity trackedHeaderEntity)
        {
            return trackedHeaderEntity.BranchCode;
        }

        if (Find(headerEntityType, headerIdValue) is BaseHeaderEntity persistedHeaderEntity)
        {
            return persistedHeaderEntity.BranchCode;
        }

        return null;
    }

    private static string NormalizeBranchCode(string? branchCode)
    {
        return string.IsNullOrWhiteSpace(branchCode) ? "0" : branchCode.Trim();
    }
}
