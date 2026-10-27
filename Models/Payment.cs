using System.ComponentModel.DataAnnotations;

namespace InvoicePro.Models;

public class Payment
{
    public int PaymentId { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [Required]
    [StringLength(30)]
    public string Method { get; set; } = "Bank Transfer";

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }
}