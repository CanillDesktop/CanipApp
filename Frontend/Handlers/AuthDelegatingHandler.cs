using Microsoft.Extensions.Logging;
using Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Frontend.Services
{
    public class AuthDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<AuthDelegatingHandler> _logger;

        public AuthDelegatingHandler(ILogger<AuthDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Requisição HTTP {Method} {RequestUri}", request.Method, request.RequestUri);

            var path = request.RequestUri?.AbsolutePath ?? "";
            var isLoginFlow = path.Contains("/api/login", StringComparison.OrdinalIgnoreCase);

            if (isLoginFlow)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            await EnsureRequestBodyBufferedAsync(request, cancellationToken);

            await AttachBearerFromSecureStorageAsync(request);

            var response = await base.SendAsync(request, cancellationToken);
            _logger.LogDebug(
                "Requisição HTTP {Method} {RequestUri} -> {StatusCode}",
                request.Method,
                request.RequestUri,
                (int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var body401 = await BufferResponseContentAsync(response, cancellationToken);
                _logger.LogWarning(
                    "Não autorizado (401) na API. Trecho da resposta: {Snippet}",
                    TruncateForMessage(body401, 400));
                _logger.LogInformation("Tentando renovar o token antes de falhar a requisição.");

                if (await TryRefreshTokenAsync(request, cancellationToken))
                {
                    response.Dispose();

                    var retryRequest = await CloneHttpRequestAsync(request, cancellationToken);
                    await AttachBearerFromSecureStorageAsync(retryRequest);

                    _logger.LogInformation("Renovação do token bem-sucedida; repetindo a requisição original.");
                    response = await base.SendAsync(retryRequest, cancellationToken);
                    _logger.LogDebug("Nova tentativa concluída com status {StatusCode}", (int)response.StatusCode);
                }
                else
                {
                    _logger.LogWarning("Falha na renovação do token; a sessão não é mais válida.");
                    var hint = TruncateForMessage(body401, 400);
                    throw new HttpRequestException(
                        string.IsNullOrEmpty(hint)
                            ? "Sessão expirada ou inválida. Faça login novamente."
                            : $"Sessão expirada ou inválida. Faça login novamente. (Servidor: {hint})",
                        inner: null,
                        statusCode: HttpStatusCode.Unauthorized);
                }
            }

            var responseBody = await BufferResponseContentAsync(response, cancellationToken);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Corpo da resposta (truncado): {Body}",
                    TruncateForMessage(responseBody, 500));
            }

            if (!response.IsSuccessStatusCode)
            {
                var snippet = TruncateForMessage(responseBody, 500);
                throw new HttpRequestException(
                    $"Erro do servidor: {(int)response.StatusCode} {response.ReasonPhrase}."
                    + (string.IsNullOrEmpty(snippet) ? "" : $" Detalhes: {snippet}"),
                    inner: null,
                    statusCode: response.StatusCode);
            }

            return response;
        }

        /// <summary>
        /// Armazena o conteúdo da requisição em buffer para permitir clonar a mensagem após o primeiro envio.
        /// </summary>
        private static async Task EnsureRequestBodyBufferedAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content == null)
                return;

            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var headerPairs = request.Content.Headers
                .Select(h => (h.Key, Values: h.Value.ToArray()))
                .ToList();
            request.Content.Dispose();

            var buffered = new ByteArrayContent(bytes);
            foreach (var (key, values) in headerPairs)
            {
                foreach (var v in values)
                    buffered.Headers.TryAddWithoutValidation(key, v);
            }

            request.Content = buffered;
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestAsync(
            HttpRequestMessage source,
            CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(source.Method, source.RequestUri)
            {
                Version = source.Version
            };

            foreach (var header in source.Headers)
            {
                foreach (var value in header.Value)
                    clone.Headers.TryAddWithoutValidation(header.Key, value);
            }

            if (source.Content != null)
            {
                var bytes = await source.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                var headerPairs = source.Content.Headers
                    .Select(h => (h.Key, Values: h.Value.ToArray()))
                    .ToList();
                var newContent = new ByteArrayContent(bytes);
                foreach (var (key, values) in headerPairs)
                {
                    foreach (var v in values)
                        newContent.Headers.TryAddWithoutValidation(key, v);
                }

                clone.Content = newContent;
            }

            return clone;
        }

        /// <summary>
        /// Lê o corpo para diagnóstico e restaura <see cref="HttpResponseMessage.Content"/> para consumidores posteriores.
        /// </summary>
        private static async Task<string?> BufferResponseContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.Content == null)
                return null;

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var headerPairs = response.Content.Headers
                .Select(h => (h.Key, Values: h.Value.ToArray()))
                .ToList();
            response.Content.Dispose();

            var restored = new ByteArrayContent(bytes);
            foreach (var (key, values) in headerPairs)
            {
                foreach (var v in values)
                    restored.Headers.TryAddWithoutValidation(key, v);
            }

            response.Content = restored;
            return bytes.Length == 0 ? null : Encoding.UTF8.GetString(bytes);
        }

        private async Task AttachBearerFromSecureStorageAsync(HttpRequestMessage request)
        {
            var idToken = await SecureStorage.GetAsync("id_token");

            if (!string.IsNullOrWhiteSpace(idToken))
            {
                var cleanToken = idToken.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", cleanToken);
                _logger.LogDebug("Cabeçalho Authorization definido a partir das credenciais armazenadas.");
            }
            else
            {
                request.Headers.Authorization = null;
                _logger.LogWarning("ID token ausente no armazenamento seguro; a requisição não enviará autorização Bearer.");
            }
        }

        private static string TruncateForMessage(string? text, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var t = text.Trim().Replace("\r\n", " ").Replace("\n", " ");
            return t.Length <= maxLen ? t : t.Substring(0, maxLen) + "…";
        }

        private async Task<bool> TryRefreshTokenAsync(HttpRequestMessage originalRequest, CancellationToken cancellationToken)
        {
            try
            {
                var refresh = await SecureStorage.GetAsync("refresh_token");
                if (string.IsNullOrWhiteSpace(refresh))
                {
                    _logger.LogWarning("Refresh token ausente; não é possível renovar a sessão.");
                    return false;
                }

                var baseUrl = originalRequest.RequestUri!.GetLeftPart(UriPartial.Authority);

                using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

                var response = await client.PostAsJsonAsync(
                    "api/login/refresh",
                    new { RefreshToken = refresh },
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning(
                        "Endpoint de renovação retornou {StatusCode}. {Detail}",
                        response.StatusCode,
                        TruncateForMessage(errorContent, 300));
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("O endpoint de renovação retornou corpo vazio.");
                    return false;
                }

                await SecureStorage.SetAsync("id_token", result.IdToken);
                await SecureStorage.SetAsync("access_token", result.AccessToken);
                await SecureStorage.SetAsync("auth_token", result.IdToken);

                if (!string.IsNullOrWhiteSpace(result.RefreshToken))
                    await SecureStorage.SetAsync("refresh_token", result.RefreshToken);

                _logger.LogInformation("Credenciais atualizadas após renovação bem-sucedida.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha na requisição de renovação do token.");
                return false;
            }
        }
    }
}
