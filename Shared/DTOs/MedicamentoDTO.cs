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

        public MedicamentoDTO() { }
        public MedicamentoDTO(
            int codigoId,
            PrioridadeEnum? prioridade,
            string? descricaoMedicamentos,
            DateTime? dataDeEntradaDoMedicamento,
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
            this.DescricaoMedicamentos = descricaoMedicamentos;
            this.DataDeEntradaDoMedicamento = dataDeEntradaDoMedicamento;
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

        public string? DescricaoDetalhada { get; set; }
        public string? NFe { get; set; }
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
        public int? EntradaEstoque { get; set; }
        public int? SaidaTotalEstoque { get; set; }


    }
}
