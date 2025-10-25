using System.ComponentModel.DataAnnotations;

namespace Frontend.Attributes;

public class DataNotInFutureAttribute : ValidationAttribute
{
    public DataNotInFutureAttribute()
        : base("A data não pode ser maior que a data atual.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime data)
        {
            if (data > DateTime.Now)
                return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
