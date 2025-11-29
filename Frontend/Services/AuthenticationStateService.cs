using System.Security.Claims;

namespace Frontend.Services
{
    public class AuthenticationStateService
    {
        // Evento disparado quando o estado de autenticação muda
        public event Action<bool>? AuthenticationStateChanged;

        /// <summary>
        /// Define o usuário como autenticado e salva tokens
        /// </summary>
        /// <param name="idToken">ID Token do AWS Cognito (para autenticação)</param>
        /// <param name="accessToken">Access Token do AWS Cognito (para AWS APIs)</param>
        /// <param name="refreshToken">Refresh Token para renovar sessão</param>
        /// <param name="email">Email do usuário</param>
        /// <param name="nome">Nome completo do usuário</param>
        public async Task SetAuthenticatedAsync(
            string idToken,
            string accessToken,
            string refreshToken,
            string email,
            string nome)
        {
            Console.WriteLine($"🔐 [AuthStateService] SetAuthenticatedAsync chamado");
            Console.WriteLine($"   - Email: {email}");
            Console.WriteLine($"   - Nome: {nome}");

            // ============================================================================
            // 🔥 SALVAR TODOS OS TOKENS NO SECURESTORAGE
            // ============================================================================
            await SecureStorage.SetAsync("id_token", idToken);
            await SecureStorage.SetAsync("access_token", accessToken);
            await SecureStorage.SetAsync("auth_token", idToken); // Compatibilidade (pode remover depois)
            await SecureStorage.SetAsync("refresh_token", refreshToken);

            // Salvar informações do usuário
            await SecureStorage.SetAsync("user_email", email);
            await SecureStorage.SetAsync("user_name", nome);

            Console.WriteLine($"✅ [AuthStateService] Tokens salvos no SecureStorage:");
            Console.WriteLine($"   - id_token: {idToken.Substring(0, 30)}...");
            Console.WriteLine($"   - access_token: {accessToken.Substring(0, 30)}...");
            Console.WriteLine($"   - refresh_token: {(string.IsNullOrEmpty(refreshToken) ? "NULL" : refreshToken.Substring(0, 30) + "...")}");

            // Dispara evento notificando que o usuário foi autenticado
            AuthenticationStateChanged?.Invoke(true);

            Console.WriteLine($"✅ [AuthStateService] Evento AuthenticationStateChanged(true) disparado");
        }

        /// <summary>
        /// Define o usuário como não autenticado e limpa tokens
        /// </summary>
        public async Task LogoutAsync()
        {
            Console.WriteLine($"🔓 [AuthStateService] LogoutAsync chamado");

            // Remove todos os tokens
            SecureStorage.Remove("id_token");
            SecureStorage.Remove("access_token");
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("user_email");
            SecureStorage.Remove("user_name");

            Console.WriteLine($"✅ [AuthStateService] Tokens removidos do SecureStorage");

            // Dispara evento notificando que o usuário foi desconectado
            AuthenticationStateChanged?.Invoke(false);

            Console.WriteLine($"✅ [AuthStateService] Evento AuthenticationStateChanged(false) disparado");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Retorna o ClaimsPrincipal do usuário atual
        /// </summary>
        public async Task<ClaimsPrincipal> GetUserClaimsAsync()
        {
            var idToken = await SecureStorage.GetAsync("id_token");

            if (string.IsNullOrWhiteSpace(idToken))
            {
                // Usuário não autenticado
                Console.WriteLine($"❌ [AuthStateService] GetUserClaimsAsync - Nenhum ID Token encontrado");
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var email = await SecureStorage.GetAsync("user_email") ?? "unknown@email.com";
            var nome = await SecureStorage.GetAsync("user_name") ?? "Usuário";

            Console.WriteLine($"✅ [AuthStateService] GetUserClaimsAsync - Usuário autenticado: {email}");

            // Cria claims do usuário
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
        /// Verifica se o usuário está autenticado
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var idToken = await SecureStorage.GetAsync("id_token");
            var isAuthenticated = !string.IsNullOrWhiteSpace(idToken);

            Console.WriteLine($"🔍 [AuthStateService] IsAuthenticatedAsync: {isAuthenticated}");

            return isAuthenticated;
        }
    }
}