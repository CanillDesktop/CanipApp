using Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Handlers
{
    public class AuthDelegatingHandler : DelegatingHandler
    {
        public AuthDelegatingHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await TryRefreshTokenAsync(request))
                {
                    // tenta novamente
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshTokenAsync(HttpRequestMessage originalRequest)
        {
            try
            {
                var refresh = await SecureStorage.GetAsync("refresh_token");
                if (string.IsNullOrWhiteSpace(refresh)) return false;

                // Recupera a BaseAddress dinâmica
                var baseUrl = originalRequest.RequestUri!.GetLeftPart(UriPartial.Authority);

                using var client = new HttpClient() { BaseAddress = new Uri(baseUrl) };

                var response = await client.PostAsJsonAsync("api/login/refresh", new { RefreshToken = refresh });

                var response = await client.PostAsJsonAsync("api/refresh", refreshToken);
                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result == null) return false;

                await SecureStorage.SetAsync("auth_token", result.AccessToken);

                if (!string.IsNullOrWhiteSpace(result.RefreshToken))
                    await SecureStorage.SetAsync("refresh_token", result.RefreshToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
