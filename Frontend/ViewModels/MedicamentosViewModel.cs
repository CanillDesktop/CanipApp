using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class MedicamentosViewModel : ObservableObject
    {
        private readonly HttpClient _http;

        public ObservableCollection<MedicamentoDTO> Medicamentos { get; } = new ObservableCollection<MedicamentoDTO>();
       

        [ObservableProperty]
        private bool carregando;

        public MedicamentosViewModel(HttpClient http)
        {
            _http = http;
        }

        [RelayCommand]
        public async Task CarregarProdutosAsync()
        {
            Carregando = true;

            var medicamentos = await _http.GetFromJsonAsync<MedicamentoDTO[]>("api/medicamentos");

            Medicamentos.Clear();
            foreach (var p in medicamentos ?? [])
                Medicamentos.Add(p);

            Carregando = false;
        }


        public async Task CadastrarProdutosAsync()
        {
            var medicamentos = await _http.PostAsJsonAsync<MedicamentoDTO[]>("api/medicamentos");
        }

    }


}
