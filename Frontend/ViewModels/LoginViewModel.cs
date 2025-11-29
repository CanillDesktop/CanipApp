using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Models;
using System.Net.Http.Json;
using Frontend.Services;

namespace Frontend.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authProvider;

        // --- PROPRIEDADES ---
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
            CustomAuthenticationStateProvider authProvider)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _authProvider = authProvider;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            LimparCacheCommand = new RelayCommand(LimparCache); // NOVO - Debug
        }

        public IAsyncRelayCommand LoginCommand { get; }
        public IRelayCommand NavigateToRegisterCommand { get; }
        public IRelayCommand LimparCacheCommand { get; } // NOVO - Debug

        private async Task LoginAsync()
        {
            // Validação básica
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

                Console.WriteLine($"🔐 [Login] Tentando autenticar: {Login}");

                var response = await _httpClient.PostAsJsonAsync("api/login", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();

                    // ============================================================================
                    // 🔥 VALIDAÇÕES DA RESPOSTA
                    // ============================================================================
                    if (result?.Token == null)
                    {
                        ErrorMessage = "Resposta inválida: Token ausente";
                        Console.WriteLine("❌ [Login] Token ausente na resposta");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(result.Token.IdToken))
                    {
                        ErrorMessage = "ID Token ausente na resposta";
                        Console.WriteLine("❌ [Login] ID Token ausente");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(result.Token.AccessToken))
                    {
                        ErrorMessage = "Access Token ausente na resposta";
                        Console.WriteLine("❌ [Login] Access Token ausente");
                        return;
                    }

                    if (result.Usuario == null)
                    {
                        ErrorMessage = "Dados do usuário não retornados";
                        Console.WriteLine("❌ [Login] Dados do usuário não retornados");
                        return;
                    }

                    // ============================================================================
                    // 🔥 LOGS DE DEBUG - TOKENS RECEBIDOS
                    // ============================================================================
                    Console.WriteLine($"✅ [Login] Login bem-sucedido para: {result.Usuario.Email}");
                    Console.WriteLine($"📋 [Login] Tokens recebidos:");
                    Console.WriteLine($"   - IdToken: {result.Token.IdToken.Substring(0, Math.Min(50, result.Token.IdToken.Length))}...");
                    Console.WriteLine($"   - IdToken Length: {result.Token.IdToken.Length}");
                    Console.WriteLine($"   - AccessToken: {result.Token.AccessToken.Substring(0, Math.Min(50, result.Token.AccessToken.Length))}...");
                    Console.WriteLine($"   - AccessToken Length: {result.Token.AccessToken.Length}");
                    Console.WriteLine($"   - RefreshToken: {(string.IsNullOrEmpty(result.Token.RefreshToken) ? "NULL" : result.Token.RefreshToken.Substring(0, 30) + "...")}");

                    // ============================================================================
                    // 🔥 SALVAR TOKENS VIA AUTHENTICATION STATE PROVIDER
                    // ============================================================================
                    await _authProvider.MarkUserAsAuthenticated(
                        result.Token.IdToken,           // ✅ ID TOKEN (para autenticação)
                        result.Token.AccessToken,       // ✅ ACCESS TOKEN (para AWS APIs)
                        result.Token.RefreshToken ?? string.Empty,
                        result.Usuario.Email ?? "N/A",
                        result.Usuario.NomeCompleto() ?? "Usuário"
                    );

                    // ============================================================================
                    // 🔥 VALIDAÇÃO PÓS-SALVAMENTO
                    // ============================================================================
                    var idTokenSalvo = await SecureStorage.GetAsync("id_token");
                    var accessTokenSalvo = await SecureStorage.GetAsync("access_token");
                    var authTokenSalvo = await SecureStorage.GetAsync("auth_token");
                    var refreshTokenSalvo = await SecureStorage.GetAsync("refresh_token");

                    Console.WriteLine($"✅ [Login] Tokens salvos no SecureStorage:");
                    Console.WriteLine($"   - id_token: {(string.IsNullOrEmpty(idTokenSalvo) ? "❌ NULL/EMPTY" : "✅ " + idTokenSalvo.Substring(0, 30) + "...")}");
                    Console.WriteLine($"   - access_token: {(string.IsNullOrEmpty(accessTokenSalvo) ? "❌ NULL/EMPTY" : "✅ " + accessTokenSalvo.Substring(0, 30) + "...")}");
                    Console.WriteLine($"   - auth_token: {(string.IsNullOrEmpty(authTokenSalvo) ? "❌ NULL/EMPTY" : "✅ " + authTokenSalvo.Substring(0, 30) + "...")}");
                    Console.WriteLine($"   - refresh_token: {(string.IsNullOrEmpty(refreshTokenSalvo) ? "❌ NULL/EMPTY" : "✅ " + refreshTokenSalvo.Substring(0, 30) + "...")}");

                    // ============================================================================
                    // 🔥 VALIDAÇÃO CRÍTICA: ID TOKEN DEVE ESTAR PRESENTE
                    // ============================================================================
                    if (string.IsNullOrEmpty(idTokenSalvo))
                    {
                        ErrorMessage = "Erro crítico: ID Token não foi salvo. Tente novamente.";
                        Console.WriteLine("❌ [Login] ERRO CRÍTICO: ID Token não foi salvo no SecureStorage!");
                        return;
                    }

                    // Salva informações adicionais em Preferences
                    Preferences.Set("user_role", result.Usuario.Permissao.ToString());
                    Preferences.Set("user_email", result.Usuario.Email ?? string.Empty);
                    Preferences.Set("user_name", result.Usuario.NomeCompleto() ?? string.Empty);

                    Console.WriteLine($"✅ [Login] Preferences salvos:");
                    Console.WriteLine($"   - user_role: {result.Usuario.Permissao}");
                    Console.WriteLine($"   - user_email: {result.Usuario.Email}");
                    Console.WriteLine($"   - user_name: {result.Usuario.NomeCompleto()}");

                    // Limpa formulário
                    Login = string.Empty;
                    Password = string.Empty;

                    Console.WriteLine("🎉 [Login] Processo de login concluído com sucesso!");
                    Console.WriteLine("🔄 [Login] Navegando para /home...");

                    // Navega para Home
                    NavigationRequested?.Invoke("/home");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Login falhou ({response.StatusCode}): {errorContent}";
                    Console.WriteLine($"❌ [Login] Falha no login:");
                    Console.WriteLine($"   - Status: {response.StatusCode}");
                    Console.WriteLine($"   - Erro: {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Erro de conexão com servidor: {ex.Message}";
                Console.WriteLine($"❌ [Login] HttpRequestException:");
                Console.WriteLine($"   - Message: {ex.Message}");
                Console.WriteLine($"   - InnerException: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro inesperado: {ex.Message}";
                Console.WriteLine($"❌ [Login] Exception:");
                Console.WriteLine($"   - Type: {ex.GetType().Name}");
                Console.WriteLine($"   - Message: {ex.Message}");
                Console.WriteLine($"   - StackTrace: {ex.StackTrace}");
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

        // ============================================================================
        // 🔥 MÉTODO DE DEBUG - LIMPAR CACHE
        // ============================================================================
        private void LimparCache()
        {
            try
            {
                Console.WriteLine("🗑️ [Login] Limpando cache...");

                // Limpa SecureStorage
                SecureStorage.RemoveAll();

                // Limpa Preferences
                Preferences.Clear();

                ErrorMessage = "✅ Cache limpo! Faça login novamente.";
                Console.WriteLine("✅ [Login] Cache limpo com sucesso");
                Console.WriteLine("   - SecureStorage: limpo");
                Console.WriteLine("   - Preferences: limpo");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao limpar cache: {ex.Message}";
                Console.WriteLine($"❌ [Login] Erro ao limpar cache: {ex.Message}");
            }
        }
    }
}