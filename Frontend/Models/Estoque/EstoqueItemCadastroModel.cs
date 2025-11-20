using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frontend.Attributes;
using Shared.DTOs.Estoque;

namespace Frontend.Models.Estoque
{
    public partial class EstoqueItemCadastroModel : ObservableObject
    {
        private string _codItem = string.Empty;
        private string? _lote = string.Empty;
        private int _quantidade = 0;
        private DateTime _dataEntrega = DateTime.Now;
        private string? _nFe;
        private DateTime? _dataValidade;

        public int IdItem { get; set; }

        [Display(Name = "Código do Item")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string CodItem 
        {
            get => _codItem;
            set
            {
                SetProperty(ref _codItem, value);
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

        public static implicit operator ItemEstoqueDTO(EstoqueItemCadastroModel model)
        {
            return new ItemEstoqueDTO
                (
                    model.IdItem,
                    model.CodItem,
                    model.Lote,
                    model.Quantidade,
                    model.DataEntrega,
                    model.NFe,
                    model.DataValidade
                );
        }
    }
}
