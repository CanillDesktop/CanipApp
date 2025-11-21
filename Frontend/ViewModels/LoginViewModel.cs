using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using System.Net.Http.Json;

namespace Frontend.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _httpClient;

    // Properties
    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private bool _carregando;
    public bool Carregando
    {
        get => _carregando;
        set => SetProperty(ref _carregando, value);
    }

    // Commands
    public IAsyncRelayCommand LoginCommand { get; }
    public IRelayCommand RegisterCommand { get; }

    // Constructor
    public LoginViewModel(NavigationManager navigationManager, IHttpClientFactory httpClientFactory)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _httpClient = httpClientFactory?.CreateClient("ApiClient") ?? throw new ArgumentNullException(nameof(httpClientFactory));

        LoginCommand = new AsyncRelayCommand(LoginAsync);
        RegisterCommand = new RelayCommand(Register);
    }

    private async Task LoginAsync()
    {
        try
        {
            // Validação de input
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                await ShowAlertAsync("Validação", "Login e senha são obrigatórios.");
                return;
            }

            if (Carregando)
                return;

            Carregando = true;

            var request = new
            {
                Login = Login.Trim(),
                Senha = Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/login", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();

                // ✅ VALIDAÇÃO CRÍTICA: Protege contra NullReferenceException
                if (result?.Token?.AccessToken == null || result?.Token?.RefreshToken == null)
                {
                    await ShowAlertAsync("Erro", "Resposta do servidor inválida. Tokens ausentes.");
                    return;
                }

                if (result.Usuario == null)
                {
                    await ShowAlertAsync("Erro", "Dados do usuário não retornados pelo servidor.");
                    return;
                }

                // ✅ Salvamento seguro
                await SecureStorage.SetAsync("auth_token", result.Token.AccessToken);
                await SecureStorage.SetAsync("refresh_token", result.Token.RefreshToken);

                Preferences.Set("user_email", result.Usuario.Email ?? "N/A");
                Preferences.Set("user_fullname", result.Usuario.NomeCompleto() ?? "Usuário");
                Preferences.Set("user_role", result.Usuario.Permissao.ToString());

                // Limpa campos após sucesso
                Login = string.Empty;
                Password = string.Empty;

                _navigationManager.NavigateTo("/home");
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                await ShowAlertAsync(
                    error?.Title ?? "Erro",
                    error?.Message ?? $"Falha no login: {response.StatusCode}"
                );
            }
        }
        catch (HttpRequestException ex)
        {
            await ShowAlertAsync("Erro de Conexão", $"Não foi possível conectar ao backend: {ex.Message}");
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Erro", $"Erro inesperado: {ex.Message}");
        }
        finally
        {
            Carregando = false;
        }
    }

    private void Register()
    {
        _navigationManager.NavigateTo("/cadastro");
    }

    // Helper seguro para exibir alertas
    private static Task ShowAlertAsync(string title, string message)
    {
        return Application.Current?.MainPage?.DisplayAlert(title, message, "OK")
            ?? Task.CompletedTask;
    }
}