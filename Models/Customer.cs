using System.ComponentModel.DataAnnotations;

namespace InvoicePro.Models;

public class Customer
{
    public int CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [StringLength(15)]
    public string? GSTIN { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Invoice> Invoices { get; set; }
        = new List<Invoice>();
}