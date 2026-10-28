using Microsoft.EntityFrameworkCore;
using InvoicePro.Data;
using InvoicePro.Repositories;
using InvoicePro.Services;
using InvoicePro.Services.Interfaces;
using InvoicePro.Filters;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InvoiceProContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<ActionLoggingFilter>();
});
builder.Services.AddScoped(
    typeof(IGenericRepository<>),
    typeof(GenericRepository<>));
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ActionLoggingFilter>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Custom route with an integer constraint.
app.MapControllerRoute(
    name: "invoice-details",
    pattern: "invoices/details/{id:int}",
    defaults: new { controller = "Invoices", action = "Details" });
      
// Custom route for invoice CSV export.
app.MapControllerRoute(
    name: "invoice-export",
    pattern: "invoices/export-csv",
    defaults: new { controller = "Invoices", action = "ExportCsv" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
