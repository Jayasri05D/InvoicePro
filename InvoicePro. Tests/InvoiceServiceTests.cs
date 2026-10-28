
using InvoicePro.Models;
using InvoicePro.Repositories;
using InvoicePro.Services;
using Moq;
using Xunit;

namespace InvoicePro.Tests;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _repository;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _repository = new Mock<IInvoiceRepository>();

        _repository
            .Setup(r => r.InvoiceNumberExistsAsync(
                It.IsAny<string>()))
            .ReturnsAsync(false);

        _repository
            .Setup(r => r.CustomerExistsAsync(
                It.IsAny<int>()))
            .ReturnsAsync(true);

        _repository
            .Setup(r => r.AddAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);

        _repository
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _service = new InvoiceService(_repository.Object);
    }

    private static Invoice CreateValidInvoice()
    {
        return new Invoice
        {
            InvoiceNumber = "INV-TEST-001",
            CustomerId = 1,
            InvoiceDate = new DateTime(2026, 10, 1),
            DueDate = new DateTime(2026, 10, 31),
            InvoiceLines = new List<InvoiceLine>
            {
                new InvoiceLine
                {
                    ProductId = 1,
                    TaxRateId = 1,
                    Quantity = 2,
                    UnitPrice = 100m,
                    TaxPercentage = 10m
                }
            }
        };
    }

    // TEST 1: Get all invoices
    [Fact]
    public async Task GetAllAsync_ReturnsInvoices()
    {
        var invoices = new List<Invoice>
        {
            CreateValidInvoice(),
            CreateValidInvoice()
        };

        _repository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(invoices);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    // TEST 2: Get an existing invoice
    [Fact]
    public async Task GetByIdAsync_WhenInvoiceExists_ReturnsInvoice()
    {
        var invoice = CreateValidInvoice();

        _repository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(invoice);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("INV-TEST-001", result.InvoiceNumber);
    }

    // TEST 3: Invoice does not exist
    [Fact]
    public async Task GetByIdAsync_WhenInvoiceDoesNotExist_ReturnsNull()
    {
        _repository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Invoice?)null);

        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // TEST 4: Reject a duplicate invoice number
    [Fact]
    public async Task CreateAsync_WhenNumberAlreadyExists_Fails()
    {
        _repository
            .Setup(r => r.InvoiceNumberExistsAsync("INV-TEST-001"))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(CreateValidInvoice());

        Assert.False(result.Success);
        _repository.Verify(
            r => r.AddAsync(It.IsAny<Invoice>()),
            Times.Never);
    }

    // TEST 5: Reject an invalid customer
    [Fact]
    public async Task CreateAsync_WhenCustomerDoesNotExist_Fails()
    {
        _repository
            .Setup(r => r.CustomerExistsAsync(1))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(CreateValidInvoice());

        Assert.False(result.Success);
    }

    // TEST 6: Reject a due date before the invoice date
    [Fact]
    public async Task CreateAsync_WhenDueDateIsBeforeInvoiceDate_Fails()
    {
        var invoice = CreateValidInvoice();
        invoice.DueDate = invoice.InvoiceDate.AddDays(-1);

        var result = await _service.CreateAsync(invoice);

        Assert.False(result.Success);
    }

    // TEST 7: Reject an invoice without lines
    [Fact]
    public async Task CreateAsync_WhenInvoiceHasNoLines_Fails()
    {
        var invoice = CreateValidInvoice();
        invoice.InvoiceLines.Clear();

        var result = await _service.CreateAsync(invoice);

        Assert.False(result.Success);
    }

    // TEST 8: Reject an invalid quantity
    [Fact]
    public async Task CreateAsync_WhenQuantityIsZero_Fails()
    {
        var invoice = CreateValidInvoice();
        invoice.InvoiceLines.First().Quantity = 0;

        var result = await _service.CreateAsync(invoice);

        Assert.False(result.Success);
    }

    // TEST 9: Reject an invalid unit price
    [Fact]
    public async Task CreateAsync_WhenUnitPriceIsZero_Fails()
    {
        var invoice = CreateValidInvoice();
        invoice.InvoiceLines.First().UnitPrice = 0m;

        var result = await _service.CreateAsync(invoice);

        Assert.False(result.Success);
    }

    // TEST 10: Reject an invalid tax percentage
    [Fact]
    public async Task CreateAsync_WhenTaxPercentageIsInvalid_Fails()
    {
        var invoice = CreateValidInvoice();
        invoice.InvoiceLines.First().TaxPercentage = -1m;

        var result = await _service.CreateAsync(invoice);

        Assert.False(result.Success);
    }

    // TEST 11: Save a valid invoice and calculate totals
    [Fact]
    public async Task CreateAsync_WhenInvoiceIsValid_SavesAndCalculatesTotals()
    {
        var invoice = CreateValidInvoice();

        var result = await _service.CreateAsync(invoice);

        Assert.True(result.Success);
        Assert.Equal(200m, invoice.Subtotal);
        Assert.Equal(20m, invoice.TaxTotal);
        Assert.Equal(220m, invoice.TotalAmount);
        Assert.Equal("Draft", invoice.Status);

        _repository.Verify(
            r => r.AddAsync(invoice),
            Times.Once);

        _repository.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }
}
