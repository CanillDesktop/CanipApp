using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Models;
using System.Net.Http.Json;
using Frontend.Services;
using Microsoft.Extensions.Logging;

namespace Frontend.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authProvider;
        private readonly ILogger<LoginViewModel> _logger;

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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public event Action<string>? NavigationRequested;

        public LoginViewModel(
            IHttpClientFactory httpClientFactory,
            CustomAuthenticationStateProvider authProvider,
            ILogger<LoginViewModel> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _authProvider = authProvider;
            _logger = logger;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            RegisterCommand = new RelayCommand(NavigateToRegister);
            LimparCacheCommand = new RelayCommand(LimparCache);
        }

        public IAsyncRelayCommand LoginCommand { get; }
        public IRelayCommand RegisterCommand { get; }
        public IRelayCommand LimparCacheCommand { get; }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Preencha email e senha";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var loginRequest = new
                {
                    Login = Login,
                    Senha = Password
                };

                _logger.LogInformation("Enviando requisição de login.");

                var response = await _httpClient.PostAsJsonAsync("api/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
                    if (result?.Token == null)
                    {
                        ErrorMessage = "Resposta inválida: Token ausente";
                        _logger.LogWarning("Resposta de login sem payload de token.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(result.Token.IdToken))
                    {
                        ErrorMessage = "ID Token ausente na resposta";
                        _logger.LogWarning("Resposta de login sem IdToken.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(result.Token.AccessToken))
                    {
                        ErrorMessage = "Access Token ausente na resposta";
                        _logger.LogWarning("Resposta de login sem AccessToken.");
                        return;
                    }

                    if (result.Usuario == null)
                    {
                        ErrorMessage = "Dados do usuário não retornados";
                        _logger.LogWarning("Resposta de login sem dados do usuário.");
                        return;
                    }

                    _logger.LogInformation(
                        "Login concluído com sucesso para {Email}.",
                        result.Usuario.Email);

                    await _authProvider.MarkUserAsAuthenticated(
                        result.Token.IdToken,
                        result.Token.AccessToken,
                        result.Token.RefreshToken ?? string.Empty,
                        result.Usuario.Email ?? "N/A",
                        result.Usuario.NomeCompleto() ?? "Usuário");

                    var idTokenSalvo = await SecureStorage.GetAsync("id_token");
                    if (string.IsNullOrEmpty(idTokenSalvo))
                    {
                        ErrorMessage = "Erro crítico: ID Token não foi salvo. Tente novamente.";
                        _logger.LogError("IdToken não foi persistido após login bem-sucedido.");
                        return;
                    }

                    Preferences.Set("user_role", result.Usuario.Permissao.ToString());
                    Preferences.Set("user_email", result.Usuario.Email ?? string.Empty);
                    Preferences.Set("user_name", result.Usuario.NomeCompleto() ?? string.Empty);

                    Login = string.Empty;
                    Password = string.Empty;

                    _logger.LogInformation("Navegando para a home após o login.");
                    NavigationRequested?.Invoke("/home");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Login falhou ({response.StatusCode}): {errorContent}";
                    _logger.LogWarning(
                        "Sign-in rejected with {StatusCode}: {Detail}",
                        response.StatusCode,
                        errorContent.Length > 500 ? errorContent.Substring(0, 500) + "…" : errorContent);
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Erro de conexão com servidor: {ex.Message}";
                _logger.LogWarning(ex, "Erro HTTP durante o login.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro inesperado: {ex.Message}";
                _logger.LogError(ex, "Erro inesperado durante o login.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateToRegister()
        {
            NavigationRequested?.Invoke("/cadastro");
        }

        /// <summary>
        /// Limpa armazenamento seguro e preferências (suporte / diagnóstico).
        /// </summary>
        private void LimparCache()
        {
            try
            {
                _logger.LogInformation("Limpando cache local de credenciais e preferências.");
                SecureStorage.RemoveAll();
                Preferences.Clear();
                ErrorMessage = "Cache limpo. Faça login novamente.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao limpar cache: {ex.Message}";
                _logger.LogWarning(ex, "Falha ao limpar o cache local.");
            }
        }
    }
}
