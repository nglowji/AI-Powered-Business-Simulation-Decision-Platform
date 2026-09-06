using BusinessTwin.Server.Modules.Inventory;
using BusinessTwin.Server.Modules.Finance;
using BusinessTwin.Server.Modules.Purchasing;
using BusinessTwin.Server.Modules.Sales;
using Microsoft.EntityFrameworkCore;

namespace BusinessTwin.Server.Shared;

public sealed class BusinessTwinDbContext(DbContextOptions<BusinessTwinDbContext> options) : DbContext(options)
{
    public DbSet<InventoryTransactionRecord> InventoryTransactions => Set<InventoryTransactionRecord>();

    public DbSet<SalesOrderRecord> SalesOrders => Set<SalesOrderRecord>();

    public DbSet<PurchaseOrderRecord> PurchaseOrders => Set<PurchaseOrderRecord>();

    public DbSet<FinancialEntryRecord> FinancialEntries => Set<FinancialEntryRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryTransactionRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => new { record.ProductId, record.WarehouseId, record.OccurredAt });
            entity.Property(record => record.SourceReference).HasMaxLength(100).IsRequired();
            entity.Property(record => record.Actor).HasMaxLength(100).IsRequired();
            entity.Property(record => record.Type).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<SalesOrderRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => new { record.CustomerId, record.CreatedAt });
            entity.Property(record => record.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(record => record.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PurchaseOrderRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => new { record.SupplierId, record.CreatedAt });
            entity.Property(record => record.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(record => record.PaymentStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(record => record.Total).HasPrecision(18, 2);
        });

        modelBuilder.Entity<FinancialEntryRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => new { record.Type, record.CreatedAt });
            entity.Property(record => record.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(record => record.Amount).HasPrecision(18, 2);
            entity.Property(record => record.Reference).HasMaxLength(100).IsRequired();
            entity.Property(record => record.Description).HasMaxLength(500);
        });
    }
}

public sealed class InventoryTransactionRecord
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid WarehouseId { get; set; }

    public InventoryTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public string SourceReference { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public string Actor { get; set; } = string.Empty;
}

public sealed class SalesOrderRecord
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public SalesOrderStatus Status { get; set; }

    public decimal Total { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class PurchaseOrderRecord
{
    public Guid Id { get; set; }

    public Guid SupplierId { get; set; }

    public PurchaseOrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public decimal Total { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class FinancialEntryRecord
{
    public Guid Id { get; set; }

    public FinancialEntryType Type { get; set; }

    public decimal Amount { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
