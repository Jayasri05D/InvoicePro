
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using InvoicePro.Data;
using InvoicePro.Models;
using InvoicePro.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InvoicePro.Controllers;

public class InvoicesController : Controller
{
    private readonly InvoiceProContext _context;
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(
        InvoiceProContext context,
        IInvoiceService invoiceService)
    {
        _context = context;
        _invoiceService = invoiceService;
    }

    // GET: Invoices
    public async Task<IActionResult> Index()
    {
        var invoices = await _invoiceService.GetAllAsync();
        return View(invoices);
    }

    // GET: Invoices/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var invoice = await _invoiceService.GetByIdAsync(id.Value);

        if (invoice == null)
            return NotFound();

        return View(invoice);
    }

    // GET: Invoices/Create
    public async Task<IActionResult> Create()
    {
        await LoadFormDataAsync();
        return View(new Invoice());
    }

    // POST: Invoices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Invoice invoice)
    {
        // Navigation properties are loaded from the database.
        ModelState.Remove(nameof(invoice.Customer));

        var lines = invoice.InvoiceLines?.ToList()
                    ?? new List<InvoiceLine>();

        // Remove validation entries for database-loaded properties.
        for (int i = 0; i < lines.Count; i++)
        {
            ModelState.Remove($"InvoiceLines[{i}].Invoice");
            ModelState.Remove($"InvoiceLines[{i}].Product");
            ModelState.Remove($"InvoiceLines[{i}].TaxRate");
            ModelState.Remove($"InvoiceLines[{i}].UnitPrice");
            ModelState.Remove($"InvoiceLines[{i}].TaxPercentage");
        }

        if (!await _context.Customers.AnyAsync(
                c => c.CustomerId == invoice.CustomerId))
        {
            ModelState.AddModelError(
                nameof(invoice.CustomerId),
                "Please select a valid customer.");
        }

        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            ModelState.AddModelError(
                nameof(invoice.InvoiceNumber),
                "Invoice number is required.");
        }
        else
        {
            invoice.InvoiceNumber = invoice.InvoiceNumber.Trim();

            if (await _context.Invoices.AnyAsync(
                    i => i.InvoiceNumber == invoice.InvoiceNumber))
            {
                ModelState.AddModelError(
                    nameof(invoice.InvoiceNumber),
                    "This invoice number already exists.");
            }
        }

        if (invoice.DueDate.Date < invoice.InvoiceDate.Date)
        {
            ModelState.AddModelError(
                nameof(invoice.DueDate),
                "Due date cannot be earlier than invoice date.");
        }

        if (lines.Count == 0)
        {
            ModelState.AddModelError(
                string.Empty,
                "Add at least one product to the invoice.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(invoice.CustomerId);
            return View(invoice);
        }

        // Load trusted product prices and tax rates from the database.
        foreach (var line in lines)
        {
            if (line.Quantity < 1 || line.Quantity > 100000)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Product quantities must be between 1 and 100000.");
                break;
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(
                    p => p.ProductId == line.ProductId);

            var taxRate = await _context.TaxRates
                .FirstOrDefaultAsync(
                    t => t.TaxRateId == line.TaxRateId);

            if (product == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "One of the selected products is invalid.");
                break;
            }

            if (!product.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Product '{product.Name}' is inactive.");
                break;
            }

            if (taxRate == null || !taxRate.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "One of the selected tax rates is invalid or inactive.");
                break;
            }

            // Never trust prices or tax percentages from the browser.
            line.Product = product;
            line.TaxRate = taxRate;
            line.UnitPrice = product.UnitPrice;
           
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(invoice.CustomerId);
            return View(invoice);
        }

        invoice.InvoiceLines = lines;

        // Calculate totals on the server.
        invoice.Subtotal = lines.Sum(line => line.LineSubtotal);
        invoice.TaxTotal = lines.Sum(line => line.TaxAmount);
        invoice.TotalAmount = invoice.Subtotal + invoice.TaxTotal;
        invoice.Status = "Draft";

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice created successfully!";

        return RedirectToAction(nameof(Index));
    }

    // Load dropdown data for the Create form.
    private async Task LoadFormDataAsync(
        int? selectedCustomerId = null)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.CustomerId = new SelectList(
            customers,
            "CustomerId",
            "Name",
            selectedCustomerId);

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewBag.Products = products;

        var taxRates = await _context.TaxRates
            .AsNoTracking()
            .Where(t => t.IsActive)
            
            .ToListAsync();

        ViewBag.TaxRates = new SelectList(
            taxRates,
            "TaxRateId",
            "Name");
    }

    // GET: Invoices/ExportCsv
    public async Task<IActionResult> ExportCsv()
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        using var memoryStream = new MemoryStream();

        await using (var writer = new StreamWriter(
            memoryStream,
            new UTF8Encoding(true),
            leaveOpen: true))
        using (var csv = new CsvWriter(
            writer,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine
            }))
        {
            csv.WriteField("Invoice Number");
            csv.WriteField("Customer");
            csv.WriteField("Invoice Date");
            csv.WriteField("Due Date");
            csv.WriteField("Subtotal");
            csv.WriteField("Tax Total");
            csv.WriteField("Total Amount");
            csv.WriteField("Status");

            await csv.NextRecordAsync();

            foreach (var invoice in invoices)
            {
                csv.WriteField(invoice.InvoiceNumber);
                csv.WriteField(invoice.Customer.Name);
                csv.WriteField(
                    invoice.InvoiceDate.ToString("yyyy-MM-dd"));
                csv.WriteField(
                    invoice.DueDate.ToString("yyyy-MM-dd"));
                csv.WriteField(invoice.Subtotal);
                csv.WriteField(invoice.TaxTotal);
                csv.WriteField(invoice.TotalAmount);
                csv.WriteField(invoice.Status);

                await csv.NextRecordAsync();
            }

            await writer.FlushAsync();
        }

        return File(
            memoryStream.ToArray(),
            "text/csv; charset=utf-8",
            $"Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
    
[AcceptVerbs("GET", "POST")]
public async Task<IActionResult> VerifyInvoiceNumber(
    string invoiceNumber,
    int invoiceId = 0)
{
    if (string.IsNullOrWhiteSpace(invoiceNumber))
        return Json(true);

    var normalizedNumber = invoiceNumber.Trim();

    var exists = await _context.Invoices.AnyAsync(i =>
        i.InvoiceNumber == normalizedNumber &&
        i.InvoiceId != invoiceId);

    return Json(!exists);
}

[HttpGet]
public async Task<IActionResult> Search(string? term)
{
    var query = _context.Invoices
        .AsNoTracking()
        .Include(i => i.Customer)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(term))
    {
        term = term.Trim();

        query = query.Where(i =>
            i.InvoiceNumber.Contains(term) ||
            i.Customer.Name.Contains(term));
    }

    var invoices = await query
        .OrderByDescending(i => i.InvoiceDate)
        .ToListAsync();

    return PartialView("_InvoiceSearchResults", invoices);
}


}
