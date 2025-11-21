using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Microsoft.AspNetCore.Components;
using Shared.DTOs;
using Shared.Enums;
using Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace Frontend.ViewModels;

public partial class CadastroViewModel : ObservableObject
{
    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private bool _carregando;
    private UsuariosModel _usuario = new();

    public UsuariosModel Usuario
    {
        get => _usuario;
        set
        {
            SetProperty(ref _usuario, value);
        }
    }

    public IAsyncRelayCommand RegisterCommand;

    public CadastroViewModel(NavigationManager navigationManager, IHttpClientFactory httpClientFactory)
    {
        _navigationManager = navigationManager;
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        RegisterCommand = new AsyncRelayCommand(RegisterAsync);
    }

    private async Task RegisterAsync()
    {
        try
        {
            if (_carregando)
                return;
            else
                _carregando = true;

            Usuario.Permissao = (int)PermissoesEnum.LEITURA;
            UsuarioRequestDTO dto = Usuario;

            var response = await _httpClient.PostAsJsonAsync("api/usuarios", dto);

            if (response.IsSuccessStatusCode)
            {
                await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Usuário cadastrado com sucesso!", "OK");

                _navigationManager.NavigateTo("/");
            }
            else
            {
                // 🔥 TRATAMENTO ROBUSTO DE ERRO
                string errorMessage = "Erro ao cadastrar usuário.";

                try
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (contentType == "application/json" || contentType == "application/problem+json")
                    {
                        // Tenta ler como JSON
                        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        if (error != null)
                        {
                            errorMessage = error.Message ?? error.Title ?? errorMessage;
                        }
                    }
                    else
                    {
                        // Se não for JSON, lê como texto
                        var textError = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(textError) && textError.Length < 500)
                        {
                            errorMessage = textError;
                        }
                    }
                }
                catch
                {
                    // Se falhar ao ler o erro, usa mensagem genérica
                    errorMessage = $"Erro {(int)response.StatusCode}: {response.ReasonPhrase}";
                }

                await Application.Current!.MainPage!.DisplayAlert("Erro", errorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert("Erro", ex.Message, "OK");
        }
        finally
        {
            Usuario = new();
            _carregando = false;
        }
    }
}