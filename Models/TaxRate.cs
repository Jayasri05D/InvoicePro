using System.ComponentModel.DataAnnotations;

namespace InvoicePro.Models;

public class TaxRate
{
    public int TaxRateId { get; set; }

    [Required]
    [StringLength(50)]
    public string TaxName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "100")]
    public decimal Rate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InvoiceLine> InvoiceLines { get; set; }
        = new List<InvoiceLine>();
}