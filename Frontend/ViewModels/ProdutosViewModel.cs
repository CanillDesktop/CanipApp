using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using Shared.Models;

namespace Frontend.ViewModels
{
    public partial class ProdutosViewModel : ObservableObject
    {
        private readonly HttpClient _http;
        private readonly NavigationManager _navigationManager;

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

        public ProdutosViewModel(IHttpClientFactory httpClientFactory, NavigationManager navigation)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            _navigationManager = navigation;
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
