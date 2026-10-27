using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoicePro.Data;

public class InvoiceProContextFactory
    : IDesignTimeDbContextFactory<InvoiceProContext>
{
    public InvoiceProContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<InvoiceProContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=InvoiceProDB;Trusted_Connection=True;TrustServerCertificate=True;");

        return new InvoiceProContext(optionsBuilder.Options);
    }
}