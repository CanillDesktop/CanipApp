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
   public class InsumosDTO
   
        {
            [Key]

            public int CodigoId { get; set; }
            public required string DescricaoSimplificada { get; set; }
            public required string DescricaoDetalhada { get; set; }
            public DateTime DataDeEntradaDoMedicamento { get; set; }
            public string? NotaFiscal { get; set; }
            //public CategoriaInsumosEnum Categoria {  get; set; }

            public UnidadeInsumosEnum Unidade { get; set; }
            public int ConsumoMensal { get; set; }
            public int ConsumoAnual { get; set; }
            public required DateOnly? ValidadeInsumo { get; set; }
            public int EstoqueDisponivel { get; set; }
            public int EntradaEstoque { get; set; }
            public int SaidaTotalEstoque { get; set; }
        }
    }


