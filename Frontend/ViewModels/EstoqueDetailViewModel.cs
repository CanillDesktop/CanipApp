using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models.Estoque;
using Shared.DTOs.Estoque;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public partial class EstoqueDetailViewModel : ObservableObject
    {
        private readonly HttpClient _http;
        private bool _carregando;
        private bool _deletando;
        private bool _retirando;

        public EstoqueItemModel? EstoqueItem { get; set; }

        public bool Carregando
        {
            get => _carregando;
            set
            {
                SetProperty(ref _carregando, value);
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

        public bool Retirando
        {
            get => _retirando;
            set
            {
                SetProperty(ref _retirando, value);
            }
        }

        public Action? OnDeleted { get; set; }
        public Action? OnRemoval { get; set; }

        public IAsyncRelayCommand<string?> DeletarLoteCommand;
        public IAsyncRelayCommand<ItemEstoqueDTO?> RetirarQtdCommand;

        public EstoqueDetailViewModel(IHttpClientFactory httpClientFactory) 
        {
            _http = httpClientFactory.CreateClient("ApiClient");
            DeletarLoteCommand = new AsyncRelayCommand<string?>(DeletarLoteAsync);
            RetirarQtdCommand = new AsyncRelayCommand<ItemEstoqueDTO?>(RetirarQuantidadeAsync);
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

        private async Task DeletarLoteAsync(string? lote)
        {
            try
            {
                if (Deletando)
                    return;

                Deletando = true;

                var isExcluir = await Application.Current!.MainPage!.DisplayAlert("Confirmação de exclusão", $"Deseja realmente excluir o lote \"{lote}\"?", "Sim", "Não");

                if (isExcluir)
                {
                    await _http.DeleteAsync($"api/estoque/{lote}");

                    var listaItens = EstoqueItem!.ItensEstoque!.ToList();
                    listaItens.Remove(EstoqueItem!.ItensEstoque!.FirstOrDefault(x => x.Lote == lote)!);
                    EstoqueItem.ItensEstoque = listaItens.ToArray();

                    OnDeleted?.Invoke();
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

        private async Task RetirarQuantidadeAsync(ItemEstoqueDTO? estoque)
        {
            try
            {
                if (Retirando)
                    return;

                Retirando = true;

                string quantidadeTexto = await Application.Current!.MainPage!.DisplayPromptAsync(
                title: "Retirar do Estoque",
                message: "Qual a QUANTIDADE que você deseja remover?",
                initialValue: "1",
                placeholder: "Digite a quantidade",
                keyboard: Keyboard.Numeric,
                maxLength: 10
                );

                if (string.IsNullOrWhiteSpace(quantidadeTexto) || quantidadeTexto == "0")
                {
                    await Application.Current.MainPage.DisplayAlert("Ação Cancelada", "Nenhuma quantidade foi informada.", "OK");
                    return;
                }

                if (!int.TryParse(quantidadeTexto, out int quantidade))
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Quantidade inválida. Por favor, digite um número inteiro.", "OK");
                    return;
                }

                if (quantidade > estoque!.Quantidade)
                {
                    await Application.Current!.MainPage!.DisplayAlert("Quantidade insuficiente no estoque", "Tentando retirar quantidade maior do que disponível no estoque", "OK");
                    return;
                }

                string nomeDestinatario = await Application.Current.MainPage.DisplayPromptAsync(
                title: "Nome do Destinatário",
                message: $"Para quem irá a quantidade de {quantidade} itens?",
                initialValue: "",
                placeholder: "Digite o nome",
                keyboard: Keyboard.Text
                );

                if (!string.IsNullOrWhiteSpace(nomeDestinatario))
                {
                    estoque.Quantidade -= quantidade;

                    if (estoque.Quantidade == 0)
                    {
                        await _http.DeleteAsync($"api/estoque/{estoque.Lote}");
                        var listaItens = EstoqueItem!.ItensEstoque!.ToList();
                        listaItens.Remove(estoque);
                        EstoqueItem.ItensEstoque = listaItens.ToArray();
                    }
                    else
                    {
                        await _http.PutAsJsonAsync($"api/estoque/{estoque.Lote}", estoque);
                    }

                    var retirada = new RetiradaEstoqueLogModel()
                    {
                        CodItem = estoque.CodItem,
                        NomeItem = EstoqueItem!.NomeItem,
                        Lote = estoque.Lote!,
                        Quantidade = quantidade,
                        De = Preferences.Get("user_fullname", ""),
                        Para = nomeDestinatario
                    };

                    await _http.PostAsJsonAsync("api/retiradaEstoque", retirada);

                    await Application.Current.MainPage.DisplayAlert(
                        "Retirada Confirmada",
                        $"Retirado(s) {quantidade} iten(s) para: {nomeDestinatario}.",
                        "OK");

                    OnRemoval?.Invoke();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ação Cancelada", "O nome do destinatário não foi informado. A operação foi cancelada.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
            }
            finally
            {
                Retirando = false;
            }
        }

        #endregion
    }
}
