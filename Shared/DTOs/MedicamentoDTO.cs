using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.DTOs
{
   public class MedicamentoDTO
    {
        [Key]
        [JsonIgnore]
        public int CodigoId { get; set; }
        public PrioridadeEnum Prioridade { get; set; }
        public required String DescricaoMedicamentos { get; set; }
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
    }
}
