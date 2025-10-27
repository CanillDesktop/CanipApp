using Shared.Enums;
using Shared.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.DTOs
{
    public class InsumosDTO : IEstoqueItem
    {
        [Key]
        public int CodigoId { get; set; }
        public required string DescricaoSimplificada { get; set; }
        public required string DescricaoDetalhada { get; set; }
        public DateTime DataDeEntradaDoMedicamento { get; set; }
        public string? NotaFiscal { get; set; }
        public UnidadeInsumosEnum Unidade { get; set; }
        public int ConsumoMensal { get; set; }
        public int ConsumoAnual { get; set; }
        public required DateOnly? ValidadeInsumo { get; set; }
        public int EstoqueDisponivel { get; set; }
        public int EntradaEstoque { get; set; }
        public int SaidaTotalEstoque { get; set; }
        public int NivelMinimoEstoque { get; set; }

        [JsonIgnore] public string Nome => DescricaoDetalhada;
        [JsonIgnore] public int QuantidadeAtual => EstoqueDisponivel;
        [JsonIgnore] public DateTime? DataDeValidade => ValidadeInsumo?.ToDateTime(TimeOnly.MinValue);
    }
}
