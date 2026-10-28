
using System.ComponentModel.DataAnnotations;

namespace InvoicePro.Validation;

public class NotFutureDateAttribute : ValidationAttribute
{
    public NotFutureDateAttribute()
    {
        ErrorMessage = "The date cannot be in the future.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime date)
            return true;

        return date.Date <= DateTime.Today;
    }
}
