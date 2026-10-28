
using InvoicePro.Models;
using InvoicePro.Repositories;
using InvoicePro.Services.Interfaces;

namespace InvoicePro.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Customer>> GetAllAsync()
        => _repository.SearchAsync(string.Empty);

    public Task<List<Customer>> SearchAsync(string term)
        => _repository.SearchAsync(term);

    public Task<Customer?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public async Task CreateAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var existing = await _repository.GetByIdAsync(customer.CustomerId);

        if (existing is null)
            throw new KeyNotFoundException("Customer not found.");

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.GSTIN = customer.GSTIN;
        existing.Address = customer.Address;

        _repository.Update(existing);
        await _repository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);

        if (customer is null)
            throw new KeyNotFoundException("Customer not found.");

        _repository.Delete(customer);
        await _repository.SaveChangesAsync();
    }
}
