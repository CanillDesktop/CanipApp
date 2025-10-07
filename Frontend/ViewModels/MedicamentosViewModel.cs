using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.DTOs;
using Shared.Enums;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class MedicamentosViewModel : ObservableObject
    {
        private readonly HttpClient _http;

        public ObservableCollection<MedicamentoDTO> Medicamentos { get; } = new ObservableCollection<MedicamentoDTO>();

        private bool _carregando;

        public bool Carregando
        {
            get => _carregando;
            set
            {
                SetProperty(ref _carregando, value);
            }
        }

        public IAsyncRelayCommand CarregarProdutosCommand;
        public IAsyncRelayCommand CriarProdutosCommand;
        public IAsyncRelayCommand DeletarProdutosCommand;

        public MedicamentosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarProdutosCommand = new AsyncRelayCommand(CarregarProdutosAsync);
            CriarProdutosCommand = new AsyncRelayCommand(CriarProdutosAsync);
            DeletarProdutosCommand = new AsyncRelayCommand(DeletarProdutosAsync);
        }

        private async Task CarregarProdutosAsync()
        {
            Carregando = true;


            var medicamentos = await _http.GetFromJsonAsync<MedicamentoDTO[]>("api/medicamentos");

            Medicamentos.Clear();
            foreach (var p in medicamentos ?? [])
                Medicamentos.Add(p);

            Carregando = false;
        }

        private async Task CriarProdutosAsync()
        {
            Carregando = true;

            var medicamentoCriado = new MedicamentoDTO
            {
                Prioridade = PrioridadeEnum.Media, 
                DescricaoMedicamentos = "Analgésico e antitérmico indicado para o alívio de dores e febre.",
                DataDeEntradaDoMedicamento = DateTime.Now, 
                NotaFiscal = "NF-987654", 
                NomeComercial = "Dipirona Monoidratada 500mg",
                PublicoAlvo = PublicoAlvoMedicamentoEnum.HumanoEAnimal,


                ConsumoMensal = 0,
                ConsumoAnual = 0,
                EntradaEstoque = 200,
                EstoqueDisponivel = 200,
                SaidaTotalEstoque = 0,


                ValidadeMedicamento = new DateOnly(2027, 10, 01)
            };

            var response = await _http.PostAsJsonAsync("api/medicamentos", medicamentoCriado);
        }

        private async Task DeletarProdutosAsync()
        {
            Carregando = true;


            int medicamentoId = 5;


            var response = await _http.DeleteAsync($"api/medicamentos/{medicamentoId}");
        }

    }
}
