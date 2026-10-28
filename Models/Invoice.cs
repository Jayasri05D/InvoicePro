using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace InvoicePro.Models;

public class Invoice: IValidatableObject
{
    public int InvoiceId { get; set; }

    [Required]
    [StringLength(30)]
    [Remote(
    action: "VerifyInvoiceNumber",
    controller: "Invoices",
    AdditionalFields = nameof(InvoiceId),
    ErrorMessage = "This invoice number already exists.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

    [StringLength(30)]
    public string Status { get; set; } = "Draft";

    public decimal Subtotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal TotalAmount { get; set; }

    public ICollection<InvoiceLine> InvoiceLines { get; set; }
        = new List<InvoiceLine>();

    public ICollection<Payment> Payments { get; set; }
        = new List<Payment>();
        
public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
{
    if (DueDate.Date < InvoiceDate.Date)
    {
        yield return new ValidationResult(
            "Due date cannot be earlier than invoice date.",
            new[] { nameof(DueDate) });
    }
}

}