
using InvoicePro.Models;
using InvoicePro.Repositories;

namespace InvoicePro.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;

    public InvoiceService(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Invoice>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Invoice?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public async Task<(bool Success, string Message)> CreateAsync(Invoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
            return (false, "Invoice number is required.");

        if (await _repository.InvoiceNumberExistsAsync(invoice.InvoiceNumber))
            return (false, "This invoice number already exists.");

        if (!await _repository.CustomerExistsAsync(invoice.CustomerId))
            return (false, "Please select a valid customer.");

        if (invoice.DueDate.Date < invoice.InvoiceDate.Date)
            return (false, "Due date cannot be before the invoice date.");

        if (invoice.InvoiceLines == null || !invoice.InvoiceLines.Any())
            return (false, "Add at least one product to the invoice.");

        foreach (var line in invoice.InvoiceLines)
        {
            if (line.Quantity <= 0)
                return (false, "Product quantity must be greater than zero.");

            if (line.UnitPrice <= 0)
                return (false, "Product price must be greater than zero.");

            if (line.TaxPercentage < 0 || line.TaxPercentage > 100)
                return (false, "Tax percentage must be between 0 and 100.");
        }

        invoice.Subtotal = invoice.InvoiceLines.Sum(line => line.LineSubtotal);
        invoice.TaxTotal = invoice.InvoiceLines.Sum(line => line.TaxAmount);
        invoice.TotalAmount = invoice.Subtotal + invoice.TaxTotal;
        invoice.Status = "Draft";

        await _repository.AddAsync(invoice);
        await _repository.SaveChangesAsync();

        return (true, "Invoice created successfully.");
    }
}
