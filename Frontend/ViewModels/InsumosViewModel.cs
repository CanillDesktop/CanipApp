using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes; // Para [DataNotInFuture] e [ValuesFromEnum]
using Shared.DTOs;        // Para o InsumosDTO
using Shared.Enums;      // Para UnidadeInsumosEnum
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public partial class InsumosModel : ObservableObject
    {
        // Backing Fields baseados no InsumosModel do Backend
        public int CodigoId {  get; set; }
        public string? _descricaoSimplificada;
        public string? _descricaoDetalhada;
        public DateTime? _dataDeEntradaDoMedicamento = DateTime.Now;
        public string? _notaFiscal;
        public UnidadeInsumosEnum _unidade;
        public int _consumoMensal;
        public int _consumoAnual;
        public DateTime? _validadeInsumo; // Frontend usa DateTime? para o DatePicker
        public int _estoqueDisponivel;
        public int _entradaEstoque; // Equivalente ao "Quantidade" do ProdutoModel
        public int SaidaTotalEstoque {  get; set; }

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
        public DateTime? DataDeEntradaDoMedicamento
        {
            get => _dataDeEntradaDoMedicamento;
            set => SetProperty(ref _dataDeEntradaDoMedicamento, value);
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
        public UnidadeInsumosEnum Unidade
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
        public DateTime? ValidadeInsumo
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
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int EntradaEstoque
        {
            get => _entradaEstoque;
            set => SetProperty(ref _entradaEstoque, value);
        }

        /// <summary>
        /// Converte o modelo do Frontend para o DTO de transferência.
        /// (Assumindo que InsumosDTO existe em Shared.DTOs)
        /// </summary>
        public static explicit operator InsumosDTO(InsumosModel model)
        {
            // NOTA: O DTO (ou o construtor dele) deve ser capaz de aceitar
            // um DateOnly? para a validade, para corresponder ao backend.
            return new InsumosDTO(
                model.CodigoId,
                model.DescricaoSimplificada,
                model.DescricaoDetalhada,
                model.DataDeEntradaDoMedicamento,
                model.NotaFiscal,
                model.Unidade,
                model.ConsumoMensal,
                model.ConsumoAnual,
                // Converte o DateTime? do frontend para DateOnly? para o backend/DTO
                model.ValidadeInsumo.HasValue
                    ? DateOnly.FromDateTime(model.ValidadeInsumo.Value)
                    : null,
                model.EstoqueDisponivel,
                model.EntradaEstoque,
                model.SaidaTotalEstoque
            );
        }
    }
}