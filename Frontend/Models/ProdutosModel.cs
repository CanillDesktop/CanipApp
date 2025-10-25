using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes;
using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public partial class ProdutosModel : ObservableObject
    {
        public string? _descricaoSimples;
        public DateTime? _dataEntrega = DateTime.Now;
        public string? _nFe;
        public string? _descricaoDetalhada;
        public UnidadeEnum _unidade;
        public CategoriaEnum _categoria;
        public int _quantidade = 0;
        public DateTime? _validade;
        public int _estoqueDisponivel = 0;

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

        [Display(Name = "Data de Entrega")]
        [DataNotInFuture(ErrorMessage = "A data de entrega não pode ser maior que a data atual")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public DateTime? DataEntrega 
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

        [Display(Name = "Quantidade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int Quantidade 
        {
            get => _quantidade;
            set => SetProperty(ref _quantidade, value);
        }

        [Display(Name = "Data de Validade")]
        public DateTime? Validade 
        {
            get => _validade;
            set
            {
                SetProperty(ref _validade, value);
            }
        }

        [Display(Name = "Qtd em Estoque")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int EstoqueDisponivel 
        {
            get => _estoqueDisponivel;
            set
            {
                SetProperty(ref _estoqueDisponivel, value);
            }
        }

        public static explicit operator ProdutosDTO(ProdutosModel model)
        {
            return new ProdutosDTO(
                model.DescricaoSimples, 
                model.DataEntrega, 
                model.NFe, 
                model.DescricaoDetalhada, 
                model.Unidade,
                model.Categoria, 
                model.Quantidade, 
                string.IsNullOrWhiteSpace(model.Validade.ToString()) ? "indeterminado" : model.Validade.ToString(), 
                model.EstoqueDisponivel);
        }
    }
}
