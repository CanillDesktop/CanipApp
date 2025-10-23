using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs;
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

        public ObservableCollection<ProdutosDTO> Produtos { get; } = [];

        private bool _carregando;

        public bool Carregando
        {
            get => _carregando;
            set
            {
                SetProperty(ref _carregando, value);
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

        public IAsyncRelayCommand CarregarProdutosCommand;
        public IRelayCommand CadastrarProdutosCommand;

        public ProdutosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarProdutosCommand = new AsyncRelayCommand(CarregarProdutosAsync);
            CadastrarProdutosCommand = new RelayCommand(CadastrarProdutosAsync);
        }

        private async Task CarregarProdutosAsync()
        {
            try
            {
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

        private void CadastrarProdutosAsync()
        {
            HasTabs = true;
            TabsShowing.First(t => t.Name == "Cadastrar").IsVisible = true;
            ActiveTab = "Cadastrar";
        }
    }
}
