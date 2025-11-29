using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Frontend.Services
{
    /// <summary>
    /// Provider customizado que integra AuthenticationStateService com o sistema de autenticação do Blazor
    /// Permite proteção de rotas via [Authorize] e &lt;AuthorizeView&gt;
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationStateService _authService;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(AuthenticationStateService authService)
        {
            _authService = authService;

            // Registra listener para mudanças de autenticação
            _authService.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        }

        /// <summary>
        /// Retorna o estado atual de autenticação do usuário
        /// </summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = await _authService.GetUserClaimsAsync();
            _currentUser = principal;
            return new AuthenticationState(principal);
        }

        /// <summary>
        /// Handler chamado quando o estado de autenticação muda (login/logout)
        /// </summary>
        private async void HandleAuthenticationStateChanged(bool isAuthenticated)
        {
            ClaimsPrincipal principal;

            if (isAuthenticated)
            {
                // Usuário fez login - carrega claims
                principal = await _authService.GetUserClaimsAsync();
            }
            else
            {
                // Usuário fez logout - limpa claims
                principal = new ClaimsPrincipal(new ClaimsIdentity());
            }

            _currentUser = principal;

            // Notifica o Blazor que o estado mudou
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        /// <summary>
        /// Marca usuário como autenticado (chamado após login bem-sucedido)
        /// </summary>
        /// <param name="idToken">ID Token do AWS Cognito (usado para autenticação)</param>
        /// <param name="accessToken">Access Token do AWS Cognito (usado para AWS APIs)</param>
        /// <param name="refreshToken">Refresh Token para renovar sessão</param>
        /// <param name="email">Email do usuário</param>
        /// <param name="nome">Nome completo do usuário</param>
        public async Task MarkUserAsAuthenticated(
            string idToken,
            string accessToken,
            string refreshToken,
            string email,
            string nome)
        {
            Console.WriteLine($"✅ [AuthProvider] MarkUserAsAuthenticated chamado");
            Console.WriteLine($"   - Email: {email}");
            Console.WriteLine($"   - Nome: {nome}");
            Console.WriteLine($"   - IdToken length: {idToken?.Length ?? 0}");
            Console.WriteLine($"   - AccessToken length: {accessToken?.Length ?? 0}");
            Console.WriteLine($"   - RefreshToken length: {refreshToken?.Length ?? 0}");

            await _authService.SetAuthenticatedAsync(idToken, accessToken, refreshToken, email, nome);

            // O evento AuthenticationStateChanged já vai disparar HandleAuthenticationStateChanged
        }

        /// <summary>
        /// Marca usuário como não autenticado (chamado no logout)
        /// </summary>
        public async Task MarkUserAsLoggedOut()
        {
            Console.WriteLine("🔓 [AuthProvider] MarkUserAsLoggedOut chamado");

            await _authService.LogoutAsync();

            // O evento AuthenticationStateChanged já vai disparar HandleAuthenticationStateChanged
        }

        /// <summary>
        /// Obtém o ID Token atual do usuário autenticado
        /// </summary>
        public async Task<string?> GetIdTokenAsync()
        {
            return await SecureStorage.GetAsync("id_token");
        }

        /// <summary>
        /// Obtém o Access Token atual do usuário autenticado
        /// </summary>
        public async Task<string?> GetAccessTokenAsync()
        {
            return await SecureStorage.GetAsync("access_token");
        }

        /// <summary>
        /// Verifica se o usuário está autenticado
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var idToken = await SecureStorage.GetAsync("id_token");
            return !string.IsNullOrWhiteSpace(idToken);
        }
    }
}