using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Shared.Enums;
using System.Text.Json.Serialization;

namespace Backend.Models.Medicamentos
{
    
    public class MedicamentosModel
    {


        [Key]
      
        public required int CodigoId { get; set; }
        public PrioridadeEnum? Prioridade { get; set; }
        public required String? DescricaoMedicamentos { get; set; }
        public DateTime? DataDeEntradaDoMedicamento { get; set; }
        public string? NotaFiscal { get; set; }
        public required string? NomeComercial { get; set; }
        public PublicoAlvoMedicamentoEnum? PublicoAlvo { get; set; }
        public int? ConsumoMensal { get; set; }
        public int? ConsumoAnual { get; set; }
        public required DateOnly? ValidadeMedicamento { get; set; }
        public int? EstoqueDisponivel { get; set; }
        public int? EntradaEstoque {  get; set; }
        public int? SaidaTotalEstoque {  get; set; }



    }
}



