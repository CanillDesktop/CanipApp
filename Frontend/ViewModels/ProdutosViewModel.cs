using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs;
using Shared.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class ProdutosViewModel : ObservableObject, ILoadableViewModel, ITabableViewModel
    {
        private readonly HttpClient _http;
        private bool _hasTabs;
        private readonly ObservableCollection<TabItemModel> _tabsShowing =
        [
            new TabItemModel("Produtos", true),
            new TabItemModel("Cadastrar")
        ];
        private string _activeTab = "Produtos";
        private bool _carregando;
        private bool _cadastrando;
        private ProdutosModel _produto = new();


        public ObservableCollection<ProdutosDTO> Produtos { get; } = [];

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
                if(SetProperty(ref _activeTab, value))
                {
                    OnTabChanged?.Invoke();
                }
            }
        }

        public Action? OnTabChanged { get; set; }

        public ProdutosModel Produto
        {
            get => _produto;
            set
            {
                SetProperty(ref _produto, value);
            }
        }

        public IAsyncRelayCommand CarregarProdutosCommand;
        public IAsyncRelayCommand<ProdutosModel?> CadastrarProdutoCommand;

        public ProdutosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarProdutosCommand = new AsyncRelayCommand(CarregarProdutosAsync);
            CadastrarProdutoCommand = new AsyncRelayCommand<ProdutosModel?>(CadastrarProdutoAsync);
        }

        private async Task CarregarProdutosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                var produtos = await _http.GetFromJsonAsync<ProdutosDTO[]>("api/produtos");

                Produtos.Clear();
                foreach (var p in produtos ?? [])
                    Produtos.Add(p);
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
            await CarregarProdutosAsync();
        }

        public void AbreAbaCadastro()
        {
            HasTabs = true;
            TabsShowing.First(t => t.Name == "Cadastrar").IsVisible = true;
        }

        private async Task CadastrarProdutoAsync(ProdutosModel? prod)
        {
            try
            {
                if (Cadastrando)
                    return;

                var dto = (ProdutosDTO)prod;

                Cadastrando = true;
                var response = await _http.PostAsJsonAsync("api/produtos", dto);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Produto cadastrado com sucesso!", "OK");
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
                Produto = new();
                Cadastrando = false;
            }
        }
    }
}
