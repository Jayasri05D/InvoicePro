
using InvoicePro.Models;

namespace InvoicePro.Services;

public interface IInvoiceService
{
    Task<List<Invoice>> GetAllAsync();

    Task<Invoice?> GetByIdAsync(int id);

    Task<(bool Success, string Message)> CreateAsync(Invoice invoice);
}
