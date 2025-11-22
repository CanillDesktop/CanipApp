using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Components.Pages;
using Frontend.Models;
using Frontend.Models.Insumos;
using Frontend.Models.Medicamentos;
using Frontend.Records;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs.Insumos;
using Shared.DTOs.Medicamentos;
using Shared.DTOs.Produtos;
using Shared.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class InsumosViewModel : ObservableObject, ILoadableViewModel, ITabableViewModel
    {
        private readonly HttpClient _http;

        private bool _hasTabs;
        private readonly ObservableCollection<TabItemModel> _tabsShowing =
        [
            new TabItemModel("Insumos", true),
            new TabItemModel("Cadastrar")
        ];
        private string _activeTab = "Insumos";

        private bool _carregando;
        private bool _cadastrando;
        private bool _deletando;

        private InsumosModel _insumoCadastro = new();

        private InsumosFiltroModel _filtro = new();
        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;

        public ObservableCollection<InsumosLeituraDTO> Insumos { get; } = [];

        public bool Carregando
        {
            get => _carregando;
            set
            {
                SetProperty(ref _carregando, value);
            }
        }

        public bool Cadastrando
        {
            get => _cadastrando;
            set
            {
                SetProperty(ref _cadastrando, value);
            }
        }

        public bool Deletando
        {
            get => _deletando;
            set
            {
                SetProperty(ref _deletando, value);
            }
        }

        public bool HasTabs
        {
            get => _hasTabs;
            set
            {
                SetProperty(ref _hasTabs, value);
            }
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

        public InsumosModel InsumoCadastro
        {
            get => _insumoCadastro;
            set
            {
                SetProperty(ref _insumoCadastro, value);
            }
        }

        public InsumosFiltroModel Filtro
        {
            get => _filtro;
            set
            {
                SetProperty(ref _filtro, value);
            }
        }

        public string ValorPesquisa
        {
            get => _valorPesquisa;
            set
            {
                SetProperty(ref _valorPesquisa, value);
            }
        }

        public string ChavePesquisa
        {
            get => _chavePesquisa;
            set
            {
                SetProperty(ref _chavePesquisa, value);
            }
        }

        public Action? OnTabChanged { get; set; }
        public Action? OnInitialLoad { get; set; }

        public IAsyncRelayCommand CarregarInsumosCommand;
        public IAsyncRelayCommand<InsumosModel?> CadastrarInsumoCommand;
        public IAsyncRelayCommand<PesquisaProduto?> FiltrarInsumosCommand;
        public IAsyncRelayCommand<InsumosLeituraDTO?> DeletarInsumoCommand;

        public InsumosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarInsumosCommand = new AsyncRelayCommand(CarregarInsumosAsync);
            CadastrarInsumoCommand = new AsyncRelayCommand<InsumosModel?>(CadastrarInsumoAsync);
            FiltrarInsumosCommand = new AsyncRelayCommand<PesquisaProduto?>(BuscarInsumosFiltradosAsync);
            DeletarInsumoCommand = new AsyncRelayCommand<InsumosLeituraDTO?>(DeletarInsumoAsync);
        }

        #region metodos
        private async Task CarregarInsumosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                var insumos = await _http.GetFromJsonAsync<InsumosLeituraDTO[]>("api/insumos");

                Insumos.Clear();
                foreach (var i in insumos ?? [])
                    Insumos.Add(i);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
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
                if (Cadastrando)
                    return;

                var dto = (InsumosCadastroDTO)insumo;

                Cadastrando = true;
                var response = await _http.PostAsJsonAsync("api/insumos", dto);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Insumo cadastrado com sucesso!", "OK");
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    await Application.Current!.MainPage!.DisplayAlert(error!.Title, error!.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                InsumoCadastro = new();
                Cadastrando = false;
            }
        }

        private async Task BuscarInsumosFiltradosAsync(PesquisaProduto? pesquisa)
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
                    await Application.Current!.MainPage!.DisplayAlert("Aviso", "Escolha um campo e preencha o valor.", "OK");
                    return;
                }

                var url = $"api/insumos?{ChavePesquisa}={Uri.UnescapeDataString(ValorPesquisa)}";
                var insumos = await _http.GetFromJsonAsync<InsumosLeituraDTO[]>(url);

                Insumos.Clear();
                foreach (var i in insumos ?? [])
                    Insumos.Add(i);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task DeletarInsumoAsync(InsumosLeituraDTO? m)
        {
            try
            {
                if (Deletando)
                    return;

                Deletando = true;

                var isExcluir = await Application.Current!.MainPage!.DisplayAlert("Confirmação de exclusão", $"Deseja realmente excluir o insumo \"{m.NomeItem}\"?", "Sim", "Não");

                if (isExcluir)
                {
                    var result = await _http.DeleteAsync($"api/insumos/{m.IdItem}");
                    await CarregarInsumosAsync();
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Deletando = false;
            }
        }

        public static string DisplayDataEntregaRecente(InsumosLeituraDTO i) =>
            i.ItensEstoque.Length > 0 ?
            i.ItensEstoque?
            .Select(i => i.DataEntrega)?
            .OrderDescending()?
            .FirstOrDefault()
            .ToShortDateString() ?? "-"
            : "-";

        public static string DisplayDescricaoDetalhada(InsumosLeituraDTO i) => string.IsNullOrEmpty(i.DescricaoDetalhada) ? "-" : i.DescricaoDetalhada;

        public static string DisplayDataValidadeRecente(InsumosLeituraDTO i) =>
            i.ItensEstoque?
            .Select(i => i.DataValidade)?
            .OrderDescending()?
            .FirstOrDefault()?
            .ToShortDateString() ?? "-";
        #endregion
    }

}
