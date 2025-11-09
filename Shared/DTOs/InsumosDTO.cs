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
        public InsumosDTO() { }

        public InsumosDTO(
             int codigoId,
             PrioridadeEnum? prioridade,
             string? descricaoSimplificada,
             DateTime? dataDeEntradaDoInsumo,
             string? notaFiscal,
             string? nomeComercial,
             PublicoAlvoMedicamentoEnum? publicoAlvo,
             int? consumoMensal,
             int? consumoAnual,
             DateOnly? validadeMedicamento,
             int? estoqueDisponivel,
             int? entradaEstoque,
             string? nFe,
             string? descricaoDetalhada,
             int? saidaTotalEstoque)
        {
            this.CodigoId = codigoId;
            this.Prioridade = prioridade;
            this.DescricaoSimplificada = descricaoSimplificada;
            this.DataDeEntradaDoInsumo = dataDeEntradaDoInsumo;
            this.NotaFiscal = notaFiscal;
            this.NomeComercial = nomeComercial;
            this.PublicoAlvo = publicoAlvo;
            this.ConsumoMensal = consumoMensal;
            this.ConsumoAnual = consumoAnual;
            this.ValidadeMedicamento = validadeMedicamento;
            this.EstoqueDisponivel = estoqueDisponivel;
            this.EntradaEstoque = entradaEstoque;
            this.NFe = nFe;
            this.DescricaoDetalhada = descricaoDetalhada;
            this.SaidaTotalEstoque = saidaTotalEstoque;
        }


        public int CodigoId { get; set; }
            public required string DescricaoSimplificada { get; set; }
            public required string DescricaoDetalhada { get; set; }
            public DateTime? DataDeEntradaDoInsumo { get; set; }
            public string? NotaFiscal { get; set; }
            //public CategoriaInsumosEnum Categoria {  get; set; }
            public PrioridadeEnum? Prioridade { get; set; }
            public UnidadeInsumosEnum Unidade { get; set; }
            public int? ConsumoMensal { get; set; }
            public int? ConsumoAnual { get; set; }
            public required DateOnly? ValidadeInsumo { get; set; }
            public int? EstoqueDisponivel { get; set; }
            public int? EntradaEstoque { get; set; }
            public int? SaidaTotalEstoque { get; set; }
        }
    }


