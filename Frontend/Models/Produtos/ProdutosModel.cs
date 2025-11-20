using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes;
using Shared.DTOs.Produtos;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Produtos
{
    public partial class ProdutosModel : ObservableObject
    {
        private string? _descricaoSimples;
        private string? _descricaoDetalhada;
        private UnidadeEnum _unidade;
        private CategoriaEnum _categoria;

        private string? _lote = string.Empty;
        private int _quantidade = 0;
        private DateTime _dataEntrega = DateTime.Now;
        private string? _nFe = string.Empty;
        private DateTime? _dataValidade;

        private int _nivelMinimoEstoque;


        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? DescricaoSimples
        {
            get => _descricaoSimples;
            set
            {
                SetProperty(ref _descricaoSimples, value);
            }
        }

        [Display(Name = "Descrição Detalhada")]
        public string? DescricaoDetalhada
        {
            get => _descricaoDetalhada;
            set
            {
                SetProperty(ref _descricaoDetalhada, value);
            }
        }

        [Display(Name = "Unidade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]
        public UnidadeEnum Unidade
        {
            get => _unidade;
            set
            {
                SetProperty(ref _unidade, value);
            }
        }

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]

        public CategoriaEnum Categoria
        {
            get => _categoria;
            set
            {
                SetProperty(ref _categoria, value);
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

        public static explicit operator ProdutosCadastroDTO(ProdutosModel model)
        {
            return new ProdutosCadastroDTO(
                model.DescricaoSimples,
                model.DescricaoDetalhada,
                model.Unidade,
                model.Categoria,
                model.Lote,
                model.DataEntrega,
                model.NFe,
                model.DataValidade,
                model.Quantidade,
                model.NivelMinimoEstoque);
        }
    }
}
