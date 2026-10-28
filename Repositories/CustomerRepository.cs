
using InvoicePro.Data;
using InvoicePro.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoicePro.Repositories;

public class CustomerRepository
    : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(InvoiceProContext context)
        : base(context)
    {
    }

    public async Task<List<Customer>> SearchAsync(string searchTerm)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();

            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Email.Contains(term) ||
                (c.GSTIN != null && c.GSTIN.Contains(term)));
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
