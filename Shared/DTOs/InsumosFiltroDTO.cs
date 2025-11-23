using System;

namespace Shared.DTOs
{
    public class InsumosFiltroDTO
    {
        // FIX: Changed from string to int?
        public int? CodigoId { get; set; }

        // FIX: Changed from string to string? (to allow null)
        public string? DescricaoSimplificada { get; set; }

        // FIX: Changed from string to string? (to allow null)
        public string? NotaFiscal { get; set; }

        // FIX: Changed from string to int? (para o valor do Enum)
        public int? Unidade { get; set; }

        // FIX: Changed from string to DateTime?
        public DateTime? DataEntrada { get; set; }

        // FIX: Changed from string to DateOnly?
        public DateOnly? DataValidade { get; set; }
    }
}