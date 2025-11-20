using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Frontend.Models.Produtos;
using Frontend.Records;
using Frontend.ViewModels.Interfaces;
using Shared.DTOs.Produtos;
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
        private bool _deletando;

        private ProdutosModel _produtoCadastro = new();

        private ProdutosFiltroModel _filtro = new();
        private string _chavePesquisa = string.Empty;
        private string _valorPesquisa = string.Empty;

        public ObservableCollection<ProdutosLeituraDTO> Produtos { get; } = [];

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
                if(SetProperty(ref _activeTab, value))
                {
                    OnTabChanged?.Invoke();
                }
            }
        }

        public ProdutosModel ProdutoCadastro
        {
            get => _produtoCadastro;
            set
            {
                SetProperty(ref _produtoCadastro, value);
            }
        }

        public ProdutosFiltroModel Filtro
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

        public IAsyncRelayCommand CarregarProdutosCommand;
        public IAsyncRelayCommand<ProdutosModel?> CadastrarProdutoCommand;
        public IAsyncRelayCommand<PesquisaProduto?> FiltrarProdutosCommand;
        public IAsyncRelayCommand<ProdutosLeituraDTO?> DeletarProdutoCommand;

        public ProdutosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarProdutosCommand = new AsyncRelayCommand(CarregarProdutosAsync);
            CadastrarProdutoCommand = new AsyncRelayCommand<ProdutosModel?>(CadastrarProdutoAsync);
            FiltrarProdutosCommand = new AsyncRelayCommand<PesquisaProduto?>(BuscarProdutosFiltradosAsync);
            DeletarProdutoCommand = new AsyncRelayCommand<ProdutosLeituraDTO?>(DeletarProdutoAsync);
        }

        #region metodos
        private async Task CarregarProdutosAsync()
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                var produtos = await _http.GetFromJsonAsync<ProdutosLeituraDTO[]>("api/produtos");

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
            OnInitialLoad?.Invoke();
        }

        public void AbreAbaCadastro()
        {
            HasTabs = true;
            TabsShowing.First(t => t.Name == "Cadastrar").IsVisible = true;
            ActiveTab = TabsShowing.First(t => t.Name == "Cadastrar").Name;
        }

        private async Task CadastrarProdutoAsync(ProdutosModel? prod)
        {
            try
            {
                if (Cadastrando)
                    return;

                var dto = (ProdutosCadastroDTO)prod;

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
                ProdutoCadastro = new();
                Cadastrando = false;
            }
        }

        private async Task BuscarProdutosFiltradosAsync(PesquisaProduto? pesquisa)
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

                var url = $"api/produtos?{ChavePesquisa}={Uri.UnescapeDataString(ValorPesquisa)}";
                var produtos = await _http.GetFromJsonAsync<ProdutosLeituraDTO[]>(url);

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

        private async Task DeletarProdutoAsync(ProdutosLeituraDTO? p)
        {
            try
            {
                if (Deletando)
                    return;

                Deletando = true;

                var isExcluir = await Application.Current!.MainPage!.DisplayAlert("Confirmação de exclusão", $"Deseja realmente excluir o produto \"{p.NomeItem}\"?", "Sim", "Não");

                if (isExcluir)
                {
                    var result = await _http.DeleteAsync($"api/produtos/{p.IdItem}");
                    await CarregarProdutosAsync();
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

        public static string DisplayDataEntregaRecente(ProdutosLeituraDTO p) => 
            p.ItensEstoque?
            .Select(i => i.DataEntrega)?
            .OrderDescending()?
            .FirstOrDefault()
            .ToShortDateString() ?? "-";

        public static string DisplayDescricaoDetalhada(ProdutosLeituraDTO p) => string.IsNullOrEmpty(p.DescricaoDetalhada) ? "-" : p.DescricaoDetalhada;

        public static string DisplayDataValidadeRecente(ProdutosLeituraDTO p) =>
            p.ItensEstoque?
            .Select(i => i.DataValidade)?
            .OrderDescending()?
            .FirstOrDefault()?
            .ToShortDateString() ?? "-";
        #endregion
    }
}
