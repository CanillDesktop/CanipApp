using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class ProdutosViewModel : ObservableObject
    {
        private readonly HttpClient _http;

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

        public IAsyncRelayCommand CarregarProdutosCommand;

        public ProdutosViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CarregarProdutosCommand = new AsyncRelayCommand(CarregarProdutosAsync);
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
    }
}
