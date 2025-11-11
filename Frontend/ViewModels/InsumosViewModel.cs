using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs;
using Shared.Models;
using Frontend.Records;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics; // CORREÇÃO: Adicionado para Debug.WriteLine

// CORREÇÃO: Adicionado o record que estava faltando, igual ao de Medicamentos
public record PesquisaInsumos(string Chave, string Valor);

namespace Frontend.ViewModels
{
    public partial class InsumosViewModel : ObservableObject, ILoadableViewModel, ITabableViewModel
    {
        private readonly HttpClient _http;
        private bool _hasTabs;
        private readonly ObservableCollection<TabItemModel> _tabsShowing =
        [
            new TabItemModel("Insumos", true),
            // CORREÇÃO: Aba de cadastro agora é visível por padrão
            new TabItemModel("Cadastrar", true)
        ];
        private string _activeTab = "Insumos";
        private bool _carregando;
        private bool _cadastrando;
        private InsumosModel _insumo = new();
        private InsumosFiltroModel _filtro = new();
        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;

        public ObservableCollection<InsumosDTO> Insumos { get; } = [];

        public bool Carregando
        {
            get => _carregando;
            set => SetProperty(ref _carregando, value);
        }

        public bool Cadastrando
        {
            get => _cadastrando;
            set => SetProperty(ref _cadastrando, value);
        }

        public bool HasTabs
        {
            get => _hasTabs;
            set => SetProperty(ref _hasTabs, value);
        }

        public ObservableCollection<TabItemModel> TabsShowing
        {
            get => _tabsShowing;
        }

        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                if (SetProperty(ref _activeTab, value))
                {
                    OnTabChanged?.Invoke();
                }
            }
        }

        public InsumosModel Insumo
        {
            get => _insumo;
            set => SetProperty(ref _insumo, value);
        }

        public InsumosFiltroModel Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value);
        }

        public string ValorPesquisa
        {
            get => _valorPesquisa;
            set => SetProperty(ref _valorPesquisa, value);
        }

        public string ChavePesquisa
        {
            get => _chavePesquisa;
            set => SetProperty(ref _chavePesquisa, value);
        }

        public Action? OnTabChanged { get; set; }
        public Action? OnInitialLoad { get; set; }

        public IAsyncRelayCommand CarregarInsumosCommand;
        public IAsyncRelayCommand<InsumosModel?> CadastrarInsumoCommand;
        public IAsyncRelayCommand<PesquisaInsumos?> FiltrarInsumosCommand;
        public IAsyncRelayCommand SincronizarInsumosCommand { get; }

        public InsumosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarInsumosCommand = new AsyncRelayCommand(CarregarInsumosAsync);
            CadastrarInsumoCommand = new AsyncRelayCommand<InsumosModel?>(CadastrarInsumoAsync);
            SincronizarInsumosCommand = new AsyncRelayCommand(SincronizarInsumosAsyncFront);
            FiltrarInsumosCommand = new AsyncRelayCommand<PesquisaInsumos?>(BuscarInsumosFiltradosAsync);
        }

        #region Metodos Principais

        private async Task CarregarInsumosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;
                var insumos = await _http.GetFromJsonAsync<InsumosDTO[]>("api/insumos");

                Insumos.Clear();
                foreach (var i in insumos ?? [])
                    Insumos.Add(i);
            }
            catch (Exception ex)
            {
                // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine para evitar crash no reload
                Debug.WriteLine($"Erro ao carregar insumos: {ex.Message}");
            }
            finally
            {
                Carregando = false;
            }
        }

        public async Task OnLoadedAsync()
        {
            await CarregarInsumosAsync();
            OnInitialLoad?.Invoke();
        }

        public void AbreAbaCadastro()
        {
            HasTabs = true;
            TabsShowing.First(t => t.Name == "Cadastrar").IsVisible = true;
            ActiveTab = TabsShowing.First(t => t.Name == "Cadastrar").Name;
        }

        private async Task CadastrarInsumoAsync(InsumosModel? insumo)
        {
            try
            {
                if (Cadastrando || insumo == null)
                    return;

                var dto = (InsumosDTO)insumo;

                Cadastrando = true;
                var response = await _http.PostAsJsonAsync("api/insumos", dto);

                if (response.IsSuccessStatusCode)
                {
                    // CORREÇÃO: Lógica alinhada com MedicamentosViewModel
                    Debug.WriteLine("Insumo cadastrado com sucesso!");
                    var itemSalvo = await response.Content.ReadFromJsonAsync<InsumosDTO>();
                    if (itemSalvo != null) Insumos.Add(itemSalvo);

                    // Volta para a aba de listagem
                    ActiveTab = "Insumos";
                    OnTabChanged?.Invoke();
                }
                else
                {
                    // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    Debug.WriteLine($"Erro: {error?.Title} - {error?.Message}");
                }
            }
            catch (Exception ex)
            {
                // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine
                Debug.WriteLine($"Erro ao cadastrar insumo: {ex.Message}");
            }
            finally
            {
                Insumo = new(); // Limpa o formulário
                Cadastrando = false;
            }
        }

        private async Task BuscarInsumosFiltradosAsync(PesquisaInsumos? pesquisa)
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                if (pesquisa == null)
                    return;

                if (string.IsNullOrWhiteSpace(ChavePesquisa) || string.IsNullOrWhiteSpace(ValorPesquisa))
                {
                    // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine
                    Debug.WriteLine("Aviso: Escolha um campo e preencha o valor.");
                    await CarregarInsumosAsync(); // Carrega todos se o filtro for inválido
                    return;
                }

                // A sua lógica de filtro com EscapeDataString já estava correta!
                var url = $"api/insumos?{ChavePesquisa}={Uri.EscapeDataString(ValorPesquisa)}";
                var insumos = await _http.GetFromJsonAsync<InsumosDTO[]>(url);

                Insumos.Clear();
                foreach (var i in insumos ?? [])
                    Insumos.Add(i);
            }
            catch (Exception ex)
            {
                // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine
                Debug.WriteLine($"Erro ao filtrar insumos: {ex.Message}");
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task SincronizarInsumosAsyncFront()
        {
            try
            {
                await _http.PostAsync("api/Sync", null);
                // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine
                Debug.WriteLine("Sincronização iniciada.");
            }
            catch (Exception ex)
            {
                // CORREÇÃO: Substituído DisplayAlert por Debug.WriteLine
                Debug.WriteLine($"Erro ao sincronizar: {ex.Message}");
            }
        }

        #endregion

        #region Metodos Auxiliares de Display

        public static string DisplayNotaFiscal(InsumosDTO i) => string.IsNullOrEmpty(i.NotaFiscal) ? "-" : i.NotaFiscal;

        public static string DisplayDescricaoDetalhada(InsumosDTO i) => string.IsNullOrEmpty(i.DescricaoDetalhada) ? "-" : i.DescricaoDetalhada;

        public static string DisplayValidade(InsumosDTO i) =>
            i.ValidadeInsumo.HasValue
            ? i.ValidadeInsumo.Value.ToShortDateString()
            : "-";

        // Esta função já estava correta
        public static string DisplayDataEntrada(InsumosDTO i) =>
            i.DataDeEntradaDoInsumo?.ToString("d") ?? "-";

        #endregion
    }
}