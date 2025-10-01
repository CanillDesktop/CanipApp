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

        public ObservableCollection<ProdutosDTO> Produtos { get; } = new ObservableCollection<ProdutosDTO>();


        [ObservableProperty]
        private bool carregando;

        public ProdutosViewModel(HttpClient http)
        {
            _http = http;
        }

        [RelayCommand]
        public async Task CarregarProdutosAsync()
        {
            Carregando = true;

            var produtos = await _http.GetFromJsonAsync<ProdutosDTO[]>("api/medicamentos");

            Produtos.Clear();
            foreach (var p in produtos ?? [])
                Produtos.Add(p);

            Carregando = false;
        }

       


    }


}
