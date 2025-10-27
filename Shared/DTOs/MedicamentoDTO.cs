using Shared.Enums;
using Shared.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shared.DTOs
{
    public class MedicamentoDTO : IEstoqueItem
    {
        [Key]
        public int CodigoId { get; set; }
        public PrioridadeEnum Prioridade { get; set; }
        public required string DescricaoMedicamentos { get; set; }
        public DateTime DataDeEntradaDoMedicamento { get; set; }
        public string? NotaFiscal { get; set; }
        public required string NomeComercial { get; set; }
        public PublicoAlvoMedicamentoEnum PublicoAlvo { get; set; }
        public int ConsumoMensal { get; set; }
        public int ConsumoAnual { get; set; }
        public required DateOnly? ValidadeMedicamento { get; set; }
        public int EstoqueDisponivel { get; set; }
        public int EntradaEstoque { get; set; }
        public int SaidaTotalEstoque { get; set; }
        public int NivelMinimoEstoque { get; set; }

        [JsonIgnore] public string Nome => NomeComercial;
        [JsonIgnore] public int QuantidadeAtual => EstoqueDisponivel;
        [JsonIgnore] public DateTime? DataDeValidade => ValidadeMedicamento?.ToDateTime(TimeOnly.MinValue);
    }
}
