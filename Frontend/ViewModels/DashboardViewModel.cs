using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Services;
using Shared.DTOs;
using Shared.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Frontend.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly HttpClient _http;
        private readonly ServicoDeEstoque _servicoDeEstoque;

        [ObservableProperty]
        private bool _carregando;

        public ObservableCollection<IEstoqueItem> ItensComEstoqueBaixo { get; } = new();
        public ObservableCollection<IEstoqueItem> ItensComVencimentoProximo { get; } = new();

        public DashboardViewModel(IHttpClientFactory httpClientFactory, ServicoDeEstoque servicoDeEstoque)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            _servicoDeEstoque = servicoDeEstoque;
        }

        public async Task CarregarAlertasAsync()
        {
            Carregando = true;
            var listaCombinada = new List<IEstoqueItem>();

            try
            {
                var produtos = await _http.GetFromJsonAsync<List<ProdutosDTO>>("api/produtos");
                var medicamentos = await _http.GetFromJsonAsync<List<MedicamentoDTO>>("api/medicamentos");
                var insumos = await _http.GetFromJsonAsync<List<InsumosDTO>>("api/insumos");

                if (produtos != null) listaCombinada.AddRange(produtos);
                if (medicamentos != null) listaCombinada.AddRange(medicamentos);
                if (insumos != null) listaCombinada.AddRange(insumos);

                var alertasEstoque = _servicoDeEstoque.VerificarEstoqueBaixo(listaCombinada);
                var alertasVencimento = _servicoDeEstoque.VerificarVencimentoProximo(listaCombinada);

                ItensComEstoqueBaixo.Clear();
                foreach (var item in alertasEstoque) ItensComEstoqueBaixo.Add(item);

                ItensComVencimentoProximo.Clear();
                foreach (var item in alertasVencimento) ItensComVencimentoProximo.Add(item);
            }
            finally
            {
                Carregando = false;
            }
        }
    }
}
