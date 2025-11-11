using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models.Insumos
{
    [DynamoDBTable("Insumos")]
    public class InsumosModel
    {
        [Key]
        [DynamoDBHashKey("Id")]
        public int CodigoId { get; set; }

        public required string DescricaoSimplificada { get; set; }
        public required string DescricaoDetalhada { get; set; }

        // CORREÇÃO: Renomeado de "DataDeEntradaDoMedicamento" e agora permite nulo
        public DateTime? DataDeEntradaDoInsumo { get; set; }

        public string? NotaFiscal { get; set; }
        //public CategoriaInsumosEnum Categoria {  get; set; }

        public UnidadeInsumosEnum Unidade { get; set; }
        public int ConsumoMensal { get; set; }
        public int ConsumoAnual { get; set; }
        public required DateOnly? ValidadeInsumo { get; set; }
        public int EstoqueDisponivel { get; set; }
        public int EntradaEstoque { get; set; }
        public int SaidaTotalEstoque { get; set; }

        public bool IsDeleted { get; set; } = false;

        [DynamoDBProperty]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}