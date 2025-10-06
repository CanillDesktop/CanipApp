using Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Frontend.Handlers
{
    public partial class AuthDelegatingHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthDelegatingHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool refreshed = await TentarRefreshTokenAsync();

                if (refreshed)
                {
                    var newToken = await SecureStorage.GetAsync("auth_token");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken!.Replace("Bearer ", ""));

                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private async Task<bool> TentarRefreshTokenAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync("refresh_token");
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return false;

                // pega um HttpClient “limpo” (sem o mesmo handler) pra não cair em loop
                using var client = new HttpClient { BaseAddress = new Uri("https://localhost:7019/") };

                var response = await client.PostAsJsonAsync("api/refresh", refreshToken);
                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (result == null)
                    return false;

                await SecureStorage.SetAsync("auth_token", result.AccessToken!);
                await SecureStorage.SetAsync("refresh_token", result.RefreshToken!);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
