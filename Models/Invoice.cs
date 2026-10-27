using System.ComponentModel.DataAnnotations;

namespace InvoicePro.Models;

public class Invoice
{
    public int InvoiceId { get; set; }

    [Required]
    [StringLength(30)]
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
}