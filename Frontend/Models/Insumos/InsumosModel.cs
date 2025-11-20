using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes;
using Shared.DTOs.Insumos;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Insumos
{
    public class InsumosModel : ObservableObject
    {
        private string? _descricaoSimplificada;
        private string? _descricaoDetalhada = string.Empty;
        private UnidadeInsumosEnum _unidade;

        private string? _lote = string.Empty;
        private int _quantidade = 0;
        private DateTime _dataEntrega = DateTime.Now;
        private string? _nFe = string.Empty;
        private DateTime? _dataValidade;

        private int _nivelMinimoEstoque;


        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? DescricaoSimplificada
        {
            get => _descricaoSimplificada;
            set
            {
                SetProperty(ref _descricaoSimplificada, value);
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
        public UnidadeInsumosEnum Unidade
        {
            get => _unidade;
            set
            {
                SetProperty(ref _unidade, value);
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

        public static explicit operator InsumosCadastroDTO(InsumosModel model)
        {
            return new InsumosCadastroDTO(
                model.DescricaoSimplificada,
                model.DescricaoDetalhada,
                model.Lote,
                model.Quantidade,
                model.DataEntrega,
                model.NFe,
                model.Unidade,
                model.DataValidade,
                model.NivelMinimoEstoque);
        }
    }
}
