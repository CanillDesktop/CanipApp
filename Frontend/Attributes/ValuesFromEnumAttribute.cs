using System;
using System.ComponentModel.DataAnnotations;

// Provavelmente o seu namespace é 'Frontend.Attributes'
namespace Frontend.Attributes
{
    public class ValuesFromEnumAttribute : ValidationAttribute
    {
        public ValuesFromEnumAttribute() : base("Valor vazio ou não permitido") { }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // 1. Se o valor for 'null', deixamos passar.
            // O atributo [Required] é quem deve (ou não) barrar valores nulos.
            if (value is null)
            {
                return ValidationResult.Success;
            }

            var property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
            var propertyType = property!.PropertyType;

            // --- ESTA É A CORREÇÃO ---
            // Se o tipo for 'Nullable<T>', pegamos o tipo 'T' (o tipo 'de baixo').
            // Se não for 'Nullable', apenas usamos o tipo da propriedade.
            var enumType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            // Agora sim verificamos se o 'enumType' é um enum
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("A propriedade a qual esse atributo é aplicado deve ser um enum");
            }
            // --- FIM DA CORREÇÃO ---

            // E usamos o 'enumType' para verificar se o valor é válido
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
}