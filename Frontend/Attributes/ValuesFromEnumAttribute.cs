using System.ComponentModel.DataAnnotations;

namespace Frontend.Attributes;
[AttributeUsage(AttributeTargets.Property)]
public class ValuesFromEnumAttribute : ValidationAttribute
{
    public ValuesFromEnumAttribute() : base("Valor vazio ou não permitido") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
        var propertyType = property!.PropertyType;

        var enumType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (!enumType.IsEnum)
        {
            throw new ArgumentException("A propriedade a qual esse atributo é aplicado deve ser um enum");
        }

        if (!Enum.IsDefined(enumType, value))
        {
            return new ValidationResult(
                FormatErrorMessage(validationContext.DisplayName),
                [validationContext.MemberName!]
            );
        }

        return ValidationResult.Success;
    }
}