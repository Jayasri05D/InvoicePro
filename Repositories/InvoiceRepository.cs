
using InvoicePro.Data;
using InvoicePro.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoicePro.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceProContext _context;

    public InvoiceRepository(InvoiceProContext context)
    {
        _context = context;
    }

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.InvoiceLines)
                .ThenInclude(line => line.Product)
            .Include(i => i.InvoiceLines)
                .ThenInclude(line => line.TaxRate)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);
    }

    public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber)
    {
        return await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        return await _context.Customers
            .AnyAsync(c => c.CustomerId == customerId);
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
