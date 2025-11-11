using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes;
using Shared.DTOs;
using Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public partial class InsumosModel : ObservableObject
    {
        // --- Backing Fields ---
        private int _codigoId;
        private string? _descricaoSimplificada;
        private string? _descricaoDetalhada;
        private DateTime? _dataDeEntradaDoInsumo = DateTime.Now;
        private string? _notaFiscal;

        // CORREÇÃO: Alterado para nullable para corresponder ao InputSelect

        private UnidadeInsumosEnum? _unidade;

        private int _consumoMensal;
        private int _consumoAnual;

        // CORREÇÃO: Alterado para DateOnly? para corresponder ao DTO e ao InputDate
        private DateOnly? _validadeInsumo;

        private int _estoqueDisponivel;
        private int _entradaEstoque;
        private int _saidaTotalEstoque;

        // --- Properties ---

        [Display(Name = "Código")]
        public int CodigoId
        {
            get => _codigoId;
            set => SetProperty(ref _codigoId, value);
        }

        [Display(Name = "Descrição Simplificada")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? DescricaoSimplificada
        {
            get => _descricaoSimplificada;
            set => SetProperty(ref _descricaoSimplificada, value);
        }

        [Display(Name = "Descrição Detalhada")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? DescricaoDetalhada
        {
            get => _descricaoDetalhada;
            set => SetProperty(ref _descricaoDetalhada, value);
        }

        [Display(Name = "Data de Entrada")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [DataNotInFuture(ErrorMessage = "A data de entrada não pode ser maior que a data atual")]
        public DateTime? DataDeEntradaDoInsumo
        {
            get => _dataDeEntradaDoInsumo;
            set => SetProperty(ref _dataDeEntradaDoInsumo, value);
        }

        [Display(Name = "Nota Fiscal")]
        public string? NotaFiscal
        {
            get => _notaFiscal;
            set => SetProperty(ref _notaFiscal, value);
        }

        [Display(Name = "Unidade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]
        // CORREÇÃO: Alterado para nullable
        public UnidadeInsumosEnum? Unidade
        {
            get => _unidade;
            set => SetProperty(ref _unidade, value);
        }

        [Display(Name = "Consumo Mensal")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int ConsumoMensal
        {
            get => _consumoMensal;
            set => SetProperty(ref _consumoMensal, value);
        }

        [Display(Name = "Consumo Anual")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int ConsumoAnual
        {
            get => _consumoAnual;
            set => SetProperty(ref _consumoAnual, value);
        }

        [Display(Name = "Data de Validade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        // CORREÇÃO: Alterado para DateOnly?
        public DateOnly? ValidadeInsumo
        {
            get => _validadeInsumo;
            set => SetProperty(ref _validadeInsumo, value);
        }

        [Display(Name = "Estoque Disponível")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int EstoqueDisponivel
        {
            get => _estoqueDisponivel;
            set => SetProperty(ref _estoqueDisponivel, value);
        }

        [Display(Name = "Entrada no Estoque")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "O valor de entrada deve ser no mínimo 1")]
        public int EntradaEstoque
        {
            get => _entradaEstoque;
            set => SetProperty(ref _entradaEstoque, value);
        }

        [Display(Name = "Saída Total do Estoque")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int SaidaTotalEstoque
        {
            get => _saidaTotalEstoque;
            set => SetProperty(ref _saidaTotalEstoque, value);
        }

        // --- Conversor para DTO ---

        public static explicit operator InsumosDTO(InsumosModel model)
        {
            return new InsumosDTO
            {
                CodigoId = model.CodigoId,
                DescricaoSimplificada = model.DescricaoSimplificada ?? string.Empty,
                DescricaoDetalhada = model.DescricaoDetalhada ?? string.Empty,
                DataDeEntradaDoInsumo = model.DataDeEntradaDoInsumo.GetValueOrDefault(),
                NotaFiscal = model.NotaFiscal,

                // CORREÇÃO: Usar GetValueOrDefault() para o Enum nullable
                Unidade = model.Unidade.GetValueOrDefault(),

                ConsumoMensal = model.ConsumoMensal,
                ConsumoAnual = model.ConsumoAnual,

                // CORREÇÃO: Os tipos agora correspondem, nenhuma conversão é necessária
                ValidadeInsumo = model.ValidadeInsumo,

                EstoqueDisponivel = model.EstoqueDisponivel,
                EntradaEstoque = model.EntradaEstoque,
                SaidaTotalEstoque = model.SaidaTotalEstoque
            };
        }
    }
}