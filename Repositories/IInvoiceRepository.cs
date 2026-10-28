
using InvoicePro.Models;

namespace InvoicePro.Repositories;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllAsync();

    Task<Invoice?> GetByIdAsync(int id);

    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber);

    Task<bool> CustomerExistsAsync(int customerId);

    Task AddAsync(Invoice invoice);

    Task SaveChangesAsync();
}
