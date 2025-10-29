using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Attributes;
using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public partial class MedicamentosModel : ObservableObject
    {
        private int _codigoId;
        private PrioridadeEnum _prioridade;
        private string? _descricaoMedicamentos;
        private DateTime _dataDeEntradaDoMedicamento = DateTime.Now;
        private string? _notaFiscal;
        private string? _nomeComercial;
        private PublicoAlvoMedicamentoEnum _publicoAlvo;
        private int _consumoMensal;
        private int _consumoAnual;
        private DateTime? _validadeMedicamento;
        private int _estoqueDisponivel;
        private int _entradaEstoque;
        private int _saidaTotalEstoque;

        [Display(Name = "Código")]
        public int CodigoId
        {
            get => _codigoId;
            set => SetProperty(ref _codigoId, value);
        }

        [Display(Name = "Prioridade")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]
        public PrioridadeEnum Prioridade
        {
            get => _prioridade;
            set => SetProperty(ref _prioridade, value);
        }

        [Display(Name = "Descrição do Medicamento")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? DescricaoMedicamentos
        {
            get => _descricaoMedicamentos;
            set => SetProperty(ref _descricaoMedicamentos, value);
        }

        [Display(Name = "Data de Entrada")]
        [DataNotInFuture(ErrorMessage = "A data não pode ser maior que a atual")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public DateTime DataDeEntradaDoMedicamento
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

        [Display(Name = "Nome Comercial")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        public string? NomeComercial
        {
            get => _nomeComercial;
            set => SetProperty(ref _nomeComercial, value);
        }

        [Display(Name = "Público Alvo")]
        [Required(ErrorMessage = "O campo '{0}' é obrigatório")]
        [ValuesFromEnum(ErrorMessage = "Valor inválido para o campo '{0}'")]
        public PublicoAlvoMedicamentoEnum PublicoAlvo
        {
            get => _publicoAlvo;
            set => SetProperty(ref _publicoAlvo, value);
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

        [Display(Name = "Validade do Medicamento")]
        public DateTime? ValidadeMedicamento
        {
            get => _validadeMedicamento;
            set => SetProperty(ref _validadeMedicamento, value);
        }

        [Display(Name = "Estoque Disponível")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int EstoqueDisponivel
        {
            get => _estoqueDisponivel;
            set => SetProperty(ref _estoqueDisponivel, value);
        }

        [Display(Name = "Entrada em Estoque")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
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

        public static explicit operator MedicamentoDTO(MedicamentosModel model)
        {
            return new MedicamentoDTO
            {
                CodigoId = model.CodigoId,
                Prioridade = model.Prioridade,
                DescricaoMedicamentos = model.DescricaoMedicamentos ?? string.Empty,
                DataDeEntradaDoMedicamento = model.DataDeEntradaDoMedicamento,
                NotaFiscal = model.NotaFiscal,
                NomeComercial = model.NomeComercial ?? string.Empty,
                PublicoAlvo = model.PublicoAlvo,
                ConsumoMensal = model.ConsumoMensal,
                ConsumoAnual = model.ConsumoAnual,
                ValidadeMedicamento = model.ValidadeMedicamento.HasValue
                    ? DateOnly.FromDateTime(model.ValidadeMedicamento.Value)
                    : null,
                EstoqueDisponivel = model.EstoqueDisponivel,
                EntradaEstoque = model.EntradaEstoque,
                SaidaTotalEstoque = model.SaidaTotalEstoque
            };
        }
    }
}
