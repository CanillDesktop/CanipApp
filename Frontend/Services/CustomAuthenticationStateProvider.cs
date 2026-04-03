using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Frontend.Services
{
    /// <summary>
    /// Integra <see cref="AuthenticationStateService"/> ao fluxo de autenticação do Blazor (rotas com [Authorize] e AuthorizeView).
    /// </summary>
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationStateService _authService;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(
            AuthenticationStateService authService,
            ILogger<CustomAuthenticationStateProvider> logger)
        {
            _authService = authService;
            _logger = logger;
            _authService.AuthenticationStateChanged += HandleAuthenticationStateChanged;
        }

        /// <summary>Retorna o estado atual de autenticação.</summary>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var principal = await _authService.GetUserClaimsAsync();
            _currentUser = principal;
            return new AuthenticationState(principal);
        }

        /// <summary>Reage a login ou logout propagados pelo <see cref="AuthenticationStateService"/>.</summary>
        private async void HandleAuthenticationStateChanged(bool isAuthenticated)
        {
            ClaimsPrincipal principal;

            if (isAuthenticated)
            {
                principal = await _authService.GetUserClaimsAsync();
            }
            else
            {
                principal = new ClaimsPrincipal(new ClaimsIdentity());
            }

            _currentUser = principal;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        /// <summary>Persiste tokens e dados do usuário após login bem-sucedido.</summary>
        public async Task MarkUserAsAuthenticated(
            string idToken,
            string accessToken,
            string refreshToken,
            string email,
            string nome)
        {
            _logger.LogInformation("Marcando usuário como autenticado: {Email} ({Nome}).", email, nome);
            _logger.LogDebug(
                "Tamanhos dos tokens: IdToken {IdLen}, AccessToken {AccessLen}, RefreshToken {RefreshLen}.",
                idToken?.Length ?? 0,
                accessToken?.Length ?? 0,
                refreshToken?.Length ?? 0);

            await _authService.SetAuthenticatedAsync(idToken, accessToken, refreshToken, email, nome);
        }

        /// <summary>Limpa a sessão (logout).</summary>
        public async Task MarkUserAsLoggedOut()
        {
            _logger.LogInformation("Marcando usuário como desconectado (logout).");
            await _authService.LogoutAsync();
        }

        /// <summary>Obtém o ID token atual do armazenamento seguro.</summary>
        public async Task<string?> GetIdTokenAsync()
        {
            return await SecureStorage.GetAsync("id_token");
        }

        /// <summary>Obtém o access token atual do armazenamento seguro.</summary>
        public async Task<string?> GetAccessTokenAsync()
        {
            return await SecureStorage.GetAsync("access_token");
        }

        /// <summary>Indica se existe ID token não vazio.</summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var idToken = await SecureStorage.GetAsync("id_token");
            return !string.IsNullOrWhiteSpace(idToken);
        }
    }
}
