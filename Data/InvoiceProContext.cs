using InvoicePro.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoicePro.Data;

public class InvoiceProContext : DbContext
{
    public InvoiceProContext(
        DbContextOptions<InvoiceProContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Invoice numbers must be unique.
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        // Indexes for common search and ageing queries.
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Name);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name);

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.DueDate);

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.Status);

        // Use two decimal places for monetary values.
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Subtotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceLine>()
            .Property(l => l.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceLine>()
            .Property(l => l.TaxPercentage)
            .HasPrecision(5, 2);

        modelBuilder.Entity<TaxRate>()
            .Property(t => t.Rate)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // Customer -> Invoices: one-to-many.
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice -> InvoiceLines: one-to-many.
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(l => l.Invoice)
            .WithMany(i => i.InvoiceLines)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> InvoiceLines: one-to-many.
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(l => l.Product)
            .WithMany(p => p.InvoiceLines)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaxRate -> InvoiceLines: one-to-many.
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(l => l.TaxRate)
            .WithMany(t => t.InvoiceLines)
            .HasForeignKey(l => l.TaxRateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice -> Payments: one-to-many.
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}