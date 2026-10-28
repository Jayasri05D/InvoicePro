
using InvoicePro.Models;
using InvoicePro.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoicePro.Controllers;

public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var customers = await _customerService.SearchAsync(search ?? "");

        ViewBag.Search = search ?? "";
        return View(customers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid)
            return View(customer);

        await _customerService.CreateAsync(customer);

        TempData["Success"] = "Customer created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);

        if (customer is null)
            return NotFound();

        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.CustomerId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(customer);

        try
        {
            await _customerService.UpdateAsync(customer);
            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _customerService.DeleteAsync(id);
            TempData["Success"] = "Customer deleted successfully.";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
