using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoicePro.Models;

public class InvoiceLine
{
    public int InvoiceLineId { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Required]
    public int TaxRateId { get; set; }

    public TaxRate TaxRate { get; set; } = null!;

    [Range(1, 100000)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal UnitPrice { get; set; }

    public decimal TaxPercentage { get; set; }

    [NotMapped]
    public decimal LineSubtotal => Quantity * UnitPrice;

    [NotMapped]
    public decimal TaxAmount =>
        Math.Round(LineSubtotal * TaxPercentage / 100m, 2);

    [NotMapped]
    public decimal LineTotal => LineSubtotal + TaxAmount;
}