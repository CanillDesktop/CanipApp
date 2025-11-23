using Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class InsumosDTO
    {
        public int CodigoId { get; set; }

        [Required(ErrorMessage = "A descrição simplificada é obrigatória")]
        // CORREÇÃO: 'required' removido
        public string DescricaoSimplificada { get; set; }

        // CORREÇÃO: 'required' removido
        public string DescricaoDetalhada { get; set; }

        public DateTime? DataDeEntradaDoInsumo { get; set; }

        public string? NotaFiscal { get; set; }

        public UnidadeInsumosEnum Unidade { get; set; }

        public int ConsumoMensal { get; set; }

        public int ConsumoAnual { get; set; }

        [Required(ErrorMessage = "A validade é obrigatória")]
        // CORREÇÃO: 'required' removido
        public DateOnly? ValidadeInsumo { get; set; }

        public int EstoqueDisponivel { get; set; }

        public int EntradaEstoque { get; set; }

        public int SaidaTotalEstoque { get; set; }
    }
}