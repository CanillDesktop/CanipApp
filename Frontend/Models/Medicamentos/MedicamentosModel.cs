using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes;
using Shared.DTOs.Medicamentos;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Medicamentos
{
    public partial class MedicamentosModel : ObservableObject
    {
        private PrioridadeEnum _prioridade;
        private string _descricaoMedicamento = string.Empty;
        private string _formula = string.Empty;
        private string _nomeComercial = string.Empty;
        private PublicoAlvoMedicamentoEnum _publicoAlvo;

        private string? _lote = string.Empty;
        private int _quantidade = 0;
        private DateTime _dataEntrega = DateTime.Now;
        private string? _nFe = string.Empty;
        private DateTime? _dataValidade;

        private int _nivelMinimoEstoque;

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string DescricaoMedicamento
        {
            get => _descricaoMedicamento;
            set
            {
                SetProperty(ref _descricaoMedicamento, value);
            }
        }

        [Display(Name = "Nome Comercial")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string NomeComercial
        {
            get => _nomeComercial ?? string.Empty;
            set
            {
                SetProperty(ref _nomeComercial, value);
            }
        }

        [Display(Name = "Fórmula")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string Formula
        {
            get => _formula;
            set
            {
                SetProperty(ref _formula, value);
            }
        }

        [Display(Name = "Prioridade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]
        public PrioridadeEnum Prioridade
        {
            get => _prioridade;
            set
            {
                SetProperty(ref _prioridade, value);
            }
        }

        [Display(Name = "Público Alvo")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]
        public PublicoAlvoMedicamentoEnum PublicoAlvo
        {
            get => _publicoAlvo;
            set
            {
                SetProperty(ref _publicoAlvo, value);
            }
        }

        [Display(Name = "Lote")]
        public string? Lote
        {
            get => _lote;
            set => SetProperty(ref _lote, value);
        }

        [Display(Name = "Quantidade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int Quantidade
        {
            get => _quantidade;
            set => SetProperty(ref _quantidade, value);
        }

        [Display(Name = "Data de Entrega")]
        [DataNotInFuture(ErrorMessage = "A data de entrega não pode ser maior que a data atual")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public DateTime DataEntrega
        {
            get => _dataEntrega;
            set
            {
                SetProperty(ref _dataEntrega, value);
            }
        }

        [Display(Name = "NFe/DOC")]
        public string? NFe
        {
            get => _nFe;
            set
            {
                SetProperty(ref _nFe, value);
            }
        }

        [Display(Name = "Data de Validade")]
        public DateTime? DataValidade
        {
            get => _dataValidade;
            set
            {
                SetProperty(ref _dataValidade, value);
            }
        }

        [Display(Name = "Nível mínimo estoque")]
        public int NivelMinimoEstoque
        {
            get => _nivelMinimoEstoque;
            set
            {
                SetProperty(ref _nivelMinimoEstoque, value);
            }
        }

        public static explicit operator MedicamentoCadastroDTO(MedicamentosModel model)
        {
            return new MedicamentoCadastroDTO(
                model.Prioridade,
                model.DescricaoMedicamento,
                model.Lote,
                model.DataEntrega,
                model.Formula,
                model.NomeComercial,
                model.PublicoAlvo,
                model.NFe,
                model.DataValidade,
                model.Quantidade,
                model.NivelMinimoEstoque);
        }
    }
}
