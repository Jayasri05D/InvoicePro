
using InvoicePro.Models;

namespace InvoicePro.Repositories;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<List<Customer>> SearchAsync(string searchTerm);
}
