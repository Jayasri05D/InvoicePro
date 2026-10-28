
using InvoicePro.Data;
using InvoicePro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InvoicePro.Controllers;

public class PaymentsController : Controller
{
    private readonly InvoiceProContext _context;

    public PaymentsController(InvoiceProContext context)
    {
        _context = context;
    }

    // GET: Payments
    public async Task<IActionResult> Index()
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return View(payments);
    }

    // GET: Payments/Create?invoiceId=1
    public async Task<IActionResult> Create(int? invoiceId)
    {
        var payment = new Payment();

        if (invoiceId.HasValue)
        {
            var invoiceExists = await _context.Invoices
                .AnyAsync(i => i.InvoiceId == invoiceId.Value);

            if (!invoiceExists)
                return NotFound();

            payment.InvoiceId = invoiceId.Value;
        }

        await LoadInvoicesAsync(payment.InvoiceId);
        return View(payment);
    }

    // POST: Payments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Payment payment)
    {
        // The invoice navigation property must come from the database.
        ModelState.Remove(nameof(payment.Invoice));

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.InvoiceId == payment.InvoiceId);

        if (invoice == null)
        {
            ModelState.AddModelError(
                nameof(payment.InvoiceId),
                "Please select a valid invoice.");
        }
        else
        {
            var paidSoFar = await _context.Payments
                .Where(p => p.InvoiceId == payment.InvoiceId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var outstanding = Math.Round(
                invoice.TotalAmount - paidSoFar, 2);

            if (invoice.TotalAmount <= 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This invoice has no payable total.");
            }
            else if (payment.Amount > outstanding)
            {
                ModelState.AddModelError(
                    nameof(payment.Amount),
                    $"Payment exceeds the outstanding balance of {outstanding:N2}.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadInvoicesAsync(payment.InvoiceId);
            return View(payment);
        }

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var totalPaid = await _context.Payments
            .Where(p => p.InvoiceId == payment.InvoiceId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        invoice.Status = totalPaid >= invoice.TotalAmount
            ? "Paid"
            : "Partially Paid";

        await _context.SaveChangesAsync();

        TempData["Success"] = "Payment recorded successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadInvoicesAsync(int? selectedInvoiceId = null)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        ViewBag.InvoiceId = new SelectList(
            invoices.Select(i => new
            {
                i.InvoiceId,
                DisplayName = $"{i.InvoiceNumber} - {i.Customer.Name} (₹{i.TotalAmount:N2})"
            }),
            "InvoiceId",
            "DisplayName",
            selectedInvoiceId);
    }
}
