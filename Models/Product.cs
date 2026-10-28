using System.ComponentModel.DataAnnotations;

namespace InvoicePro.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InvoiceLine> InvoiceLines { get; set; }
        = new List<InvoiceLine>();
}


