using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Shared.Enums;
using System.Text.Json.Serialization;

namespace Backend.Models.Medicamentos
{

    public class InsumosModel
    {


        [Key]

        public int CodigoId { get; set; }
        public required string DescricaoSimplificada { get; set; }
        public required string DescricaoDetalhada { get; set; }
        public DateTime DataDeEntradaDoMedicamento { get; set; }
        public string? NotaFiscal { get; set; }
        //public CategoriaInsumosEnum Categoria {  get; set; }
    
        public  UnidadeInsumosEnum Unidade { get; set; }
        public int ConsumoMensal { get; set; }
        public int ConsumoAnual { get; set; }
        public required DateOnly? ValidadeInsumo { get; set; }
        public int EstoqueDisponivel { get; set; }
        public int EntradaEstoque { get; set; }
        public int SaidaTotalEstoque { get; set; }



    }
}



