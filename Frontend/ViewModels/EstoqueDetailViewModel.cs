using CommunityToolkit.Mvvm.ComponentModel;
using Frontend.Models.Estoque;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class EstoqueDetailViewModel : ObservableObject
    {
        private readonly HttpClient _http;
        private bool _carregando;

        public EstoqueItemModel? EstoqueItem { get; set; }

        public bool Carregando
        {
            get => _carregando;
            set
            {
                SetProperty(ref _carregando, value);
            }
        }

        public Action? OnInitialLoad { get; set; }

        public EstoqueDetailViewModel(IHttpClientFactory httpClientFactory) 
        {
            _http = httpClientFactory.CreateClient("ApiClient");
        }

        #region metodos

        public async Task BuscarItemPorCodigoAsync(int idItem, string tipo)
        {
            try
            {
                if (Carregando)
                    return;

                Carregando = true;

                EstoqueItem = await _http.GetFromJsonAsync<EstoqueItemModel>($"api/{tipo}/{idItem}");
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

        #endregion
    }
}
