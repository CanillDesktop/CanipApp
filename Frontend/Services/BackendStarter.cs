using System.Diagnostics;
using System.Text.Json;
using Frontend.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Frontend.Services;

/// <summary>
/// Orquestração do host para a API embutida: descoberta via <c>backend.json</c>, linha de URL no stdout e encerramento.
/// </summary>
public static class BackendStarter
{
    private static readonly ILogger s_log = AppHostLogging.Create(nameof(BackendStarter));

    private static Process? _backendProcess;
    private static string? _discoveryFilePath;
    private static bool _ownedByThisProcess = false;
    private static readonly object _lock = new object();

    /// <summary>Metadados de descoberta persistidos junto à API (ver <c>backend.json</c>).</summary>
    private class BackendDiscoveryInfo
    {
        public int port { get; set; }
        public int pid { get; set; }
        public string? startedAt { get; set; }
        public string? version { get; set; }
        public string? url { get; set; }
    }

    /// <summary>Inicia a API se necessário e retorna o endereço HTTP base.</summary>
    public static string StartBackendAndGetUrl()
    {
        lock (_lock)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var canilAppPath = Path.Combine(appDataPath, "CanilApp");
            Directory.CreateDirectory(canilAppPath);

            _discoveryFilePath = Path.Combine(canilAppPath, "backend.json");

            if (File.Exists(_discoveryFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_discoveryFilePath);
                    var info = JsonSerializer.Deserialize<BackendDiscoveryInfo>(json);

                    if (info != null && info.port > 0 && info.pid > 0)
                    {
                        // Verifica se o processo ainda está vivo
                        if (IsProcessAlive(info.pid))
                        {
                            s_log.LogInformation(
                                "Instância da API já em execução (PID {Pid}, porta {Port}).",
                                info.pid,
                                info.port);

                            var url = $"http://127.0.0.1:{info.port}";
                            if (TestBackendConnection(url))
                            {
                                s_log.LogInformation("Verificação de saúde concluída para {Url}.", url);
                                _ownedByThisProcess = false;
                                return url;
                            }

                            s_log.LogWarning("Arquivo de descoberta existe, mas a API não respondeu; removendo dados obsoletos.");
                            File.Delete(_discoveryFilePath);
                        }
                        else
                        {
                            s_log.LogWarning("O processo {Pid} do arquivo de descoberta não está em execução; iniciando nova instância da API.", info.pid);
                            File.Delete(_discoveryFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    s_log.LogWarning(ex, "Falha ao ler o arquivo de descoberta da API.");
                    // Remove arquivo corrompido
                    if (File.Exists(_discoveryFilePath))
                        File.Delete(_discoveryFilePath);
                }
            }

            var appDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parentDir = Path.GetDirectoryName(appDir);
            var candidateBaseDirs = new[]
            {
                appDir,
                parentDir ?? appDir,
                Path.Combine(appDir, "Backend"),
                Path.Combine(parentDir ?? appDir, "Backend"),
                Path.GetFullPath(Path.Combine(appDir, "..", "Backend")),
                Path.Combine(appDir, "Backend", "win-x64"),
                Path.Combine(parentDir ?? appDir, "Backend", "win-x64"),
                Path.GetFullPath(Path.Combine(appDir, "..", "Backend", "win-x64"))
            };

            string? backendPath = null;
            bool isDll = false;
            var allTriedPaths = new List<string>();

            foreach (var baseDir in candidateBaseDirs)
            {
                if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
                    continue;

                var possiblePaths = new[]
                {
                    Path.Combine(baseDir, "Backend.exe"),
                    Path.Combine(baseDir, "Backend.dll"),
                    Path.Combine(baseDir, "Backend", "Backend.exe"),
                    Path.Combine(baseDir, "Backend", "Backend.dll"),
                    Path.Combine(baseDir, "win-x64", "Backend.exe"),
                    Path.Combine(baseDir, "win-x64", "Backend.dll"),
                    Path.Combine(baseDir, "Backend", "win-x64", "Backend.exe"),
                    Path.Combine(baseDir, "Backend", "win-x64", "Backend.dll")
                };

                foreach (var path in possiblePaths)
                {
                    allTriedPaths.Add(path);
                    if (File.Exists(path))
                    {
                        backendPath = path;
                        isDll = path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                        s_log.LogInformation("Executável da API localizado: {Path}", path);
                        break;
                    }
                }

                if (backendPath != null)
                    break;
            }

            if (backendPath == null)
            {
                var searchedPaths = string.Join("\n  - ", allTriedPaths.Distinct());
                throw new FileNotFoundException(
                    "Executável da API não encontrado. Confirme se a pasta Backend foi publicada junto à aplicação.\n\n" +
                    $"Diretório da aplicação: {appDir}\n\n" +
                    $"Caminhos verificados:\n  - {searchedPaths}"
                );
            }

            var psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(backendPath)
            };

            if (isDll)
            {
                psi.FileName = "dotnet";
                psi.Arguments = $"\"{backendPath}\" --urls http://127.0.0.1:0";
            }
            else
            {
                psi.FileName = backendPath;
                psi.Arguments = "--urls http://127.0.0.1:0";
            }

            s_log.LogInformation("Iniciando processo da API: {FileName} {Arguments}", psi.FileName, psi.Arguments);

            _backendProcess = Process.Start(psi);

            if (_backendProcess == null)
            {
                throw new InvalidOperationException("Não foi possível iniciar o processo da API.");
            }

            _ownedByThisProcess = true;

            s_log.LogInformation("Processo da API iniciado (PID {Pid}).", _backendProcess.Id);

            string? discoveredUrl = null;
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMinutes(30);

            var stdoutTask = Task.Run(() =>
            {
                while (!_backendProcess.StandardOutput.EndOfStream)
                {
                    var line = _backendProcess.StandardOutput.ReadLine();
                    if (line == null) continue;

                    s_log.LogDebug("[stdout da API] {Line}", line);

                    if (line.StartsWith("BACKEND_URL:", StringComparison.OrdinalIgnoreCase))
                    {
                        var url = line.Replace("BACKEND_URL:", "").Trim();

                        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        {
                            s_log.LogInformation("URL base obtida pelo stdout da API: {Url}", url);
                            return url;
                        }
                    }
                }
                return null;
            });

            while (DateTime.Now - startTime < timeout)
            {
                // Tenta ler arquivo JSON
                if (File.Exists(_discoveryFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_discoveryFilePath);
                        var info = JsonSerializer.Deserialize<BackendDiscoveryInfo>(json);

                        if (info != null && !string.IsNullOrEmpty(info.url))
                        {
                            discoveredUrl = info.url;
                            s_log.LogInformation("URL base lida do arquivo de descoberta: {Url}", discoveredUrl);
                            break;
                        }
                    }
                    catch
                    {
                        // Arquivo pode estar sendo escrito, tenta novamente
                    }
                }

                // Verifica se stdout já retornou
                if (stdoutTask.IsCompleted && stdoutTask.Result != null)
                {
                    discoveredUrl = stdoutTask.Result;
                    break;
                }

                Thread.Sleep(100);
            }

            if (discoveredUrl == null && !stdoutTask.IsCompleted)
            {
                discoveredUrl = stdoutTask.Wait(5000) ? stdoutTask.Result : null;
            }

            if (discoveredUrl == null)
            {
                _backendProcess.Kill();
                throw new TimeoutException(
                    "Tempo esgotado aguardando a publicação da URL base da API (arquivo de descoberta ou stdout).");
            }

            s_log.LogInformation("API escutando em {Url}.", discoveredUrl);

            return discoveredUrl;
        }
    }

    /// <summary>Encerra a API quando esta aplicação a iniciou (shutdown HTTP gracioso e, se preciso, término forçado).</summary>
    public static async Task ShutdownBackend()
    {
        lock (_lock)
        {
            if (_backendProcess == null || !_ownedByThisProcess)
            {
                s_log.LogDebug("Encerramento da API ignorado (não iniciada por esta sessão).");
                return;
            }

            if (_backendProcess.HasExited)
            {
                s_log.LogDebug("O processo da API já foi encerrado.");
                return;
            }

            s_log.LogInformation("Encerrando API (PID {Pid}).", _backendProcess.Id);
        }

        bool shutdownSuccess = false;

        try
        {
            // Tenta ler URL do arquivo de discovery
            if (File.Exists(_discoveryFilePath))
            {
                var json = File.ReadAllText(_discoveryFilePath);
                var info = JsonSerializer.Deserialize<BackendDiscoveryInfo>(json);

                if (info != null && !string.IsNullOrEmpty(info.url))
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var response = await client.PostAsync($"{info.url}/internal/shutdown", null);

                    if (response.IsSuccessStatusCode)
                    {
                        s_log.LogInformation("Encerramento gracioso aceito; aguardando término do processo.");
                        shutdownSuccess = _backendProcess!.WaitForExit(5000);
                        if (shutdownSuccess)
                            s_log.LogInformation("Processo da API encerrado após shutdown gracioso.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            s_log.LogWarning(ex, "Falha na requisição de encerramento gracioso.");
        }

        if (!shutdownSuccess && _backendProcess != null && !_backendProcess.HasExited)
        {
            try
            {
                s_log.LogWarning("Encerrando o processo da API à força.");
                _backendProcess.Kill();
                _backendProcess.WaitForExit(2000);
                s_log.LogInformation("Processo da API encerrado.");
            }
            catch (Exception ex)
            {
                s_log.LogWarning(ex, "Falha ao encerrar o processo da API.");
            }
        }

        _backendProcess?.Dispose();
        _backendProcess = null;
        _ownedByThisProcess = false;
    }

    private static bool IsProcessAlive(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private static bool TestBackendConnection(string url)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);

            var response = client.GetAsync($"{url}/api/health").Result;
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}