
using InvoicePro.Models;

namespace InvoicePro.Services.Interfaces;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();

    Task<List<Customer>> SearchAsync(string term);

    Task<Customer?> GetByIdAsync(int id);

    Task CreateAsync(Customer customer);

    Task UpdateAsync(Customer customer);

    Task DeleteAsync(int id);
}
