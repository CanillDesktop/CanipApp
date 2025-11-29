using Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public class AuthDelegatingHandler : DelegatingHandler
    {
        public AuthDelegatingHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"🔍 [Handler] Request: {request.Method} {request.RequestUri}");

            // ============================================================================
            // 🔥 SEMPRE USAR ID TOKEN (contém aud correto)
            // ============================================================================
            var idToken = await SecureStorage.GetAsync("id_token");
            Console.WriteLine($"🔑 [Handler] ID Token: {(string.IsNullOrWhiteSpace(idToken) ? "NULL/EMPTY" : idToken.Substring(0, 30) + "...")}");

            if (!string.IsNullOrWhiteSpace(idToken))
            {
                // Remove "Bearer " se já existir (precaução)
                var cleanToken = idToken.Replace("Bearer ", "");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);
                Console.WriteLine($"✅ [Handler] Authorization header adicionado: Bearer {cleanToken.Substring(0, 30)}...");
            }
            else
            {
                Console.WriteLine("❌ [Handler] ID Token não encontrado no SecureStorage");
            }

            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine($"📡 [Handler] Response: {(int)response.StatusCode} {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("⚠️ [Handler] 401 Unauthorized - Tentando refresh token...");

                if (await TryRefreshTokenAsync(request))
                {
                    Console.WriteLine("✅ [Handler] Refresh bem-sucedido, reenviando request");

                    // Re-adicionar ID token atualizado
                    var newIdToken = await SecureStorage.GetAsync("id_token");
                    if (!string.IsNullOrWhiteSpace(newIdToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newIdToken.Replace("Bearer ", ""));
                        Console.WriteLine($"✅ [Handler] Novo token adicionado ao retry");
                    }

                    response = await base.SendAsync(request, cancellationToken);
                    Console.WriteLine($"📡 [Handler] Response após refresh: {(int)response.StatusCode} {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine("❌ [Handler] Refresh token falhou - sessão expirada");
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshTokenAsync(HttpRequestMessage originalRequest)
        {
            try
            {
                var refresh = await SecureStorage.GetAsync("refresh_token");
                if (string.IsNullOrWhiteSpace(refresh))
                {
                    Console.WriteLine("❌ [Handler] Refresh token não encontrado");
                    return false;
                }

                var baseUrl = originalRequest.RequestUri!.GetLeftPart(UriPartial.Authority);

                using var client = new HttpClient() { BaseAddress = new Uri(baseUrl) };

                Console.WriteLine($"🔄 [Handler] Tentando refresh token...");
                var response = await client.PostAsJsonAsync("api/login/refresh", new { RefreshToken = refresh });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ [Handler] Refresh falhou: {response.StatusCode} - {errorContent}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result == null)
                {
                    Console.WriteLine("❌ [Handler] Resposta de refresh nula");
                    return false;
                }

                // ============================================================================
                // 🔥 SALVAR TODOS OS TOKENS ATUALIZADOS
                // ============================================================================
                await SecureStorage.SetAsync("id_token", result.IdToken);
                await SecureStorage.SetAsync("access_token", result.AccessToken);
                await SecureStorage.SetAsync("auth_token", result.IdToken); // Compatibilidade

                if (!string.IsNullOrWhiteSpace(result.RefreshToken))
                {
                    await SecureStorage.SetAsync("refresh_token", result.RefreshToken);
                }

                Console.WriteLine("✅ [Handler] Tokens atualizados com sucesso");
                Console.WriteLine($"   - Novo IdToken: {result.IdToken.Substring(0, 30)}...");
                Console.WriteLine($"   - Novo AccessToken: {result.AccessToken.Substring(0, 30)}...");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [Handler] Erro no refresh: {ex.Message}");
                return false;
            }
        }
    }
}