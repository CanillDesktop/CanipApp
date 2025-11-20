using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models.Estoque;
using Shared.Models;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class AddLoteEstoqueViewModel : ObservableObject
    {
        private readonly HttpClient _http;
        private bool _cadastrando;

        private EstoqueItemCadastroModel? _estoqueItemCadastro = new();

        public bool Cadastrando
        {
            get => _cadastrando;
            set
            {
                SetProperty(ref _cadastrando, value);
            }
        }

        public EstoqueItemCadastroModel? EstoqueItemCadastro
        {
            get => _estoqueItemCadastro;
            set
            {
                SetProperty(ref _estoqueItemCadastro, value);
            }
        }

        public IAsyncRelayCommand<EstoqueItemCadastroModel?> CadastrarLoteCommand;

        public AddLoteEstoqueViewModel(IHttpClientFactory httpClientFactory)
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            CadastrarLoteCommand = new AsyncRelayCommand<EstoqueItemCadastroModel?>(CadastrarNovoLoteAsync);
        }

        #region metodos

        private async Task CadastrarNovoLoteAsync(EstoqueItemCadastroModel? item)
        {
            try
            {
                if (Cadastrando)
                    return;

                Cadastrando = true;

                var response = await _http.PostAsJsonAsync("api/estoque", item);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Novo lote cadastrado com sucesso!", "OK");
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
                Cadastrando = false;
                EstoqueItemCadastro = new();
            }
        }

        #endregion
    }
}
