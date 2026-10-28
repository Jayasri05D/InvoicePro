
using InvoicePro.Data;
using InvoicePro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoicePro.Controllers;

public class ProductsController : Controller
{
    private readonly InvoiceProContext _context;

    public ProductsController(InvoiceProContext context)
    {
        _context = context;
    }

    // GET: /Products
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(products);
    }

    // GET: /Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    // GET: /Products/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
            return View(product);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Product created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var product = await _context.Products
            .FindAsync(id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    // POST: /Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("ProductId,Name,Description,UnitPrice,IsActive")]
        Product product)
    {
        if (id != product.ProductId)
            return NotFound();

        if (!ModelState.IsValid)
            return View(product);

        var existingProduct = await _context.Products
            .FindAsync(id);

        if (existingProduct == null)
            return NotFound();

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.IsActive = product.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Product updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound();

        return View(product);
    }

    // POST: /Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound();

        if (await _context.Entry(product)
            .Collection(p => p.InvoiceLines)
            .Query()
            .AnyAsync())
        {
            TempData["Error"] =
                "This product is already used in an invoice and cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Product deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
