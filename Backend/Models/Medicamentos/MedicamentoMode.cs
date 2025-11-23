using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Backend.Models.Medicamentos
{

    [DynamoDBTable("Medicamentos")]
    public class MedicamentosModel
    {


        [Key]
        [DynamoDBHashKey("id")]
        public int CodigoId { get; set; }
        public PrioridadeEnum Prioridade { get; set; }
        public required String DescricaoMedicamentos { get; set; }
        public DateTime DataDeEntradaDoMedicamento { get; set; }
        public string? NotaFiscal { get; set; }
        public required string NomeComercial { get; set; }
        
        public PublicoAlvoMedicamentoEnum PublicoAlvo { get; set; }
        public int ConsumoMensal { get; set; }
        public int ConsumoAnual { get; set; }

        [JsonIgnore]
        [DynamoDBProperty("ValidadeMedicamento")]
      
        public string ValidadeMedicamentoString => this.ValidadeMedicamento.HasValue ? this.ValidadeMedicamento.Value.ToString("yyyy-MM-dd") : null;

        [DynamoDBIgnore]
        public required DateOnly? ValidadeMedicamento { get; set; }
        public int? EstoqueDisponivel { get; set; }
        public int? EntradaEstoque {  get; set; }
        public int? SaidaTotalEstoque { get; set; }
        [DynamoDBProperty] 
        public bool IsDeleted { get; set; } = false;

        [DynamoDBProperty]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;



    }
}



