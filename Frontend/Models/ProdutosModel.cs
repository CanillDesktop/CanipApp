using CommunityToolkit.Mvvm.ComponentModel;
using Shared.DTOs;
using Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public partial class ProdutosModel : ObservableObject
    {
        private string? _descricaoSimples;
        private DateTime? _dataEntrega = DateTime.Now;
        private string? _nFe;
        private string? _descricaoDetalhada;
        private UnidadeEnum _unidade;
        private CategoriaEnum _categoria;
        private int _quantidade = 0;
        private DateTime? _validade;
        private int _estoqueDisponivel = 0;

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? DescricaoSimples
        {
            get => _descricaoSimples;
            set => SetProperty(ref _descricaoSimples, value);
        }

        [Display(Name = "Data de Entrega")]
        
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public DateTime? DataEntrega
        {
            get => _dataEntrega;
            set => SetProperty(ref _dataEntrega, value);
        }

        [Display(Name = "NFe/DOC")]
        public string? NFe
        {
            get => _nFe;
            set => SetProperty(ref _nFe, value);
        }

        [Display(Name = "Descrição Detalhada")]
        public string? DescricaoDetalhada
        {
            get => _descricaoDetalhada;
            set => SetProperty(ref _descricaoDetalhada, value);
        }

        [Display(Name = "Unidade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
       
        public UnidadeEnum Unidade
        {
            get => _unidade;
            set => SetProperty(ref _unidade, value);
        }

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
       
        public CategoriaEnum Categoria
        {
            get => _categoria;
            set => SetProperty(ref _categoria, value);
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
            set => SetProperty(ref _validade, value);
        }

        [Display(Name = "Qtd em Estoque")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int EstoqueDisponivel
        {
            get => _estoqueDisponivel;
            set => SetProperty(ref _estoqueDisponivel, value);
        }

        // ✅ Conversão segura e compatível com o DTO
        public static explicit operator ProdutosDTO(ProdutosModel model)
        {
            return new ProdutosDTO(
                model.DescricaoSimples,
                model.DataEntrega ?? DateTime.Now,              // Garante que nunca será nulo
                model.NFe,
                model.DescricaoDetalhada,
                model.Unidade,
                model.Categoria,
                model.Quantidade,
                model.Validade?.ToString("dd/MM/yyyy"),         // Converte se houver data
                model.EstoqueDisponivel
            );
        }
    }
}
