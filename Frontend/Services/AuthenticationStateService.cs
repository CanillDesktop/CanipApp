using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Frontend.Services
{
    public class AuthenticationStateService
    {
        private readonly ILogger<AuthenticationStateService> _logger;

        public AuthenticationStateService(ILogger<AuthenticationStateService> logger)
        {
            _logger = logger;
        }

        public event Action<bool>? AuthenticationStateChanged;

        /// <summary>
        /// Persiste tokens e dados de exibição do usuário e notifica os assinantes.
        /// </summary>
        public async Task SetAuthenticatedAsync(
            string idToken,
            string accessToken,
            string refreshToken,
            string email,
            string nome)
        {
            _logger.LogInformation(
                "Sessão autenticada registrada para {Email} ({Name}).",
                email,
                nome);

            await SecureStorage.SetAsync("id_token", idToken);
            await SecureStorage.SetAsync("access_token", accessToken);
            await SecureStorage.SetAsync("auth_token", idToken);
            await SecureStorage.SetAsync("refresh_token", refreshToken);

            await SecureStorage.SetAsync("user_email", email);
            await SecureStorage.SetAsync("user_name", nome);

            _logger.LogDebug(
                "Chaves de credencial gravadas (refresh token presente: {HasRefresh}).",
                !string.IsNullOrEmpty(refreshToken));

            AuthenticationStateChanged?.Invoke(true);
            _logger.LogDebug("Evento AuthenticationStateChanged disparado (autenticado).");
        }

        /// <summary>
        /// Remove credenciais armazenadas e notifica os assinantes.
        /// </summary>
        public async Task LogoutAsync()
        {
            _logger.LogInformation("Limpando sessão armazenada.");

            SecureStorage.Remove("id_token");
            SecureStorage.Remove("access_token");
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_name");

            AuthenticationStateChanged?.Invoke(false);
            _logger.LogDebug("Evento AuthenticationStateChanged disparado (sessão encerrada).");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Monta um <see cref="ClaimsPrincipal"/> a partir do armazenamento seguro (ou bypass de desenvolvimento quando ativo).
        /// </summary>
        public async Task<ClaimsPrincipal> GetUserClaimsAsync()
        {
            if (Frontend.DevAuthBypass.SkipLogin)
            {
                _logger.LogWarning("DevAuthBypass.SkipLogin está ativo; retornando principal sintético.");
                var devClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Dev (bypass)"),
                    new Claim(ClaimTypes.Email, "dev@local"),
                    new Claim("sub", "dev-bypass")
                };
                return new ClaimsPrincipal(new ClaimsIdentity(devClaims, authenticationType: "DevBypass"));
            }

            var idToken = await SecureStorage.GetAsync("id_token");

            if (string.IsNullOrWhiteSpace(idToken))
            {
                _logger.LogDebug("Sem ID token; retornando principal não autenticado.");
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var email = await SecureStorage.GetAsync("user_email") ?? "unknown@email.com";
            var nome = await SecureStorage.GetAsync("user_name") ?? "Usuário";

            _logger.LogDebug("Principal construído para {Email}.", email);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nome),
                new Claim(ClaimTypes.Email, email),
                new Claim("id_token", idToken)
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Indica se existe ID token não vazio (ou se o bypass de desenvolvimento está ativo).
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            if (Frontend.DevAuthBypass.SkipLogin)
                return true;

            var idToken = await SecureStorage.GetAsync("id_token");
            var isAuthenticated = !string.IsNullOrWhiteSpace(idToken);

            _logger.LogDebug("IsAuthenticatedAsync: {IsAuthenticated}.", isAuthenticated);

            return isAuthenticated;
        }
    }
}
