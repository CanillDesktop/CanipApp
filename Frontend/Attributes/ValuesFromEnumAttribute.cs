using System.ComponentModel.DataAnnotations;

namespace Frontend.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ValuesFromEnumAttribute : ValidationAttribute
{
    public ValuesFromEnumAttribute() : base("Valor vazio ou não permitido") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(value);

        var property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);

        if (!property!.PropertyType.IsEnum)
        {
            throw new ArgumentException("A propriedade a qual esse atributo é aplicado deve ser um enum");
        }

        if (!Enum.IsDefined(property.PropertyType, value))
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                [validationContext.MemberName!]
            );
        }

        return ValidationResult.Success;
    }
}
