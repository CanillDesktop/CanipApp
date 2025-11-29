using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Shared.DTOs.Medicamentos
{
   public class MedicamentoCadastroDTO
    {
        public MedicamentoCadastroDTO() 
        {
        }
        public MedicamentoCadastroDTO(PrioridadeEnum prioridade, string descricaoMedicamento, string? lote, DateTime dataEntrega, string formula, string nomeComercial, PublicoAlvoMedicamentoEnum publicoAlvo,
            string? nFe, DateTime? dataValidade, int quantidade = 0, int nivelMinimoEstoque = 0)
        {
            CodMedicamento = GeraIdentificador();
            Prioridade = prioridade;
            DescricaoMedicamento = descricaoMedicamento;
            Lote = lote;
            DataEntrega = dataEntrega;
            Formula = formula;
            NomeComercial = nomeComercial;
            PublicoAlvo = publicoAlvo;
            Quantidade = quantidade;
            NFe = nFe;
            DataValidade = dataValidade;
            NivelMinimoEstoque = nivelMinimoEstoque;
        }

        public int CodigoId { get; set; }

        [Display(Name = "Código")]
        public string CodMedicamento { get; set; } = string.Empty;

        [Display(Name = "Prioridade")]
        public PrioridadeEnum Prioridade { get; set; }

        [Display(Name = "Descrição")]
        public string DescricaoMedicamento { get; set; } = string.Empty;

        [Display(Name = "Lote")]
        public string? Lote { get; set; } = string.Empty;

        [Display(Name = "Quantidade")]
        public int Quantidade { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime DataEntrega { get; set; }

        [Display(Name = "NFe/DOC")]
        public string? NFe { get; set; } = string.Empty;

        [Display(Name = "Fórmula")]
        public string Formula { get; set; } = string.Empty;

        [Display(Name = "Nome Comercial")]
        public string NomeComercial { get; set; } = string.Empty;

        [Display(Name = "Público Alvo")]
        public PublicoAlvoMedicamentoEnum PublicoAlvo { get; set; }

        [Display(Name = "Data de Validade")]
        public DateTime? DataValidade { get; set; }

        [Display(Name = "Nível mínimo estoque")]
        public int NivelMinimoEstoque { get; set; }

        private static string GeraIdentificador()
        {
            var id = "MED";

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            guid = Regex.Replace(guid, @"\D", "");

            id += guid;

            return id;
        }
    }
}
