using System.Diagnostics;
using System.Text.Json;

namespace Frontend.Services;

/// <summary>
/// Serviço responsável por iniciar, descobrir e encerrar o backend ASP.NET Core
/// Implementa discovery via arquivo JSON e kill switch automático
/// </summary>
public static class BackendStarter
{
    private static Process? _backendProcess;
    private static string? _discoveryFilePath;
    private static bool _ownedByThisProcess = false;
    private static readonly object _lock = new object();

    /// <summary>
    /// Classe para deserializar o arquivo backend.json
    /// </summary>
    private class BackendDiscoveryInfo
    {
        public int port { get; set; }
        public int pid { get; set; }
        public string? startedAt { get; set; }
        public string? version { get; set; }
        public string? url { get; set; }
    }

    /// <summary>
    /// Inicia o backend (se necessário) e retorna a URL dinamicamente descoberta
    /// </summary>
    public static string StartBackendAndGetUrl()
    {
        lock (_lock)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var canilAppPath = Path.Combine(appDataPath, "CanilApp");
            Directory.CreateDirectory(canilAppPath);

            _discoveryFilePath = Path.Combine(canilAppPath, "backend.json");

            // ============================================================================
            // 🔥 ETAPA 1: VERIFICA SE JÁ EXISTE INSTÂNCIA RODANDO
            // ============================================================================
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
                            Console.WriteLine($"✅ Backend já está rodando (PID: {info.pid}, Porta: {info.port})");

                            // Tenta validar se o backend está realmente respondendo
                            var url = $"http://127.0.0.1:{info.port}";
                            if (TestBackendConnection(url))
                            {
                                Console.WriteLine($"✅ Backend validado e funcionando em: {url}");
                                _ownedByThisProcess = false; // Não foi iniciado por nós
                                return url;
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Backend não está respondendo, iniciando novo...");
                                // Remove arquivo de discovery corrompido
                                File.Delete(_discoveryFilePath);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Processo {info.pid} não existe mais, iniciando novo backend...");
                            // Remove arquivo de discovery obsoleto
                            File.Delete(_discoveryFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erro ao ler backend.json: {ex.Message}");
                    // Remove arquivo corrompido
                    if (File.Exists(_discoveryFilePath))
                        File.Delete(_discoveryFilePath);
                }
            }

            // ============================================================================
            // 🔥 ETAPA 2: LOCALIZA O EXECUTÁVEL DO BACKEND
            // ============================================================================
            var baseDir = AppContext.BaseDirectory;

            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "Backend.exe"),
                Path.Combine(baseDir, "Backend", "Backend.exe"),
                Path.Combine(baseDir, "win-x64", "Backend.exe"),
                Path.Combine(baseDir, "Backend", "win-x64", "Backend.exe"),
                Path.Combine(baseDir, "Backend.dll"),
                Path.Combine(baseDir, "Backend", "Backend.dll"),
                Path.Combine(baseDir, "win-x64", "Backend.dll"),
                Path.Combine(baseDir, "Backend", "win-x64", "Backend.dll")
            };

            string? backendPath = null;
            bool isDll = false;

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    backendPath = path;
                    isDll = path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                    Console.WriteLine($"✅ Backend encontrado em: {path}");
                    break;
                }
            }

            if (backendPath == null)
            {
                var searchedPaths = string.Join("\n  - ", possiblePaths);
                throw new FileNotFoundException(
                    $"❌ Backend não encontrado em nenhum destes locais:\n  - {searchedPaths}\n\n" +
                    $"BaseDirectory: {baseDir}"
                );
            }

            // ============================================================================
            // 🔥 ETAPA 3: CONFIGURA E INICIA O PROCESSO DO BACKEND
            // ============================================================================
            var psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(backendPath) // ✅ Define working directory
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

            Console.WriteLine($"🚀 Iniciando backend: {psi.FileName} {psi.Arguments}");

            _backendProcess = Process.Start(psi);

            if (_backendProcess == null)
            {
                throw new Exception("❌ Falha ao iniciar o backend.");
            }

            _ownedByThisProcess = true; // Marcamos que iniciamos este processo

            Console.WriteLine($"✅ Processo do backend iniciado (PID: {_backendProcess.Id})");

            // ============================================================================
            // 🔥 ETAPA 4: DESCOBERTA DINÂMICA DA PORTA
            // ============================================================================
            // Estratégia híbrida: Tenta arquivo JSON primeiro, fallback para stdout

            string? discoveredUrl = null;
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(30);

            // Thread para capturar stdout (fallback)
            var stdoutTask = Task.Run(() =>
            {
                while (!_backendProcess.StandardOutput.EndOfStream)
                {
                    var line = _backendProcess.StandardOutput.ReadLine();
                    if (line == null) continue;

                    Console.WriteLine($"[Backend] {line}");

                    if (line.StartsWith("BACKEND_URL:", StringComparison.OrdinalIgnoreCase))
                    {
                        var url = line.Replace("BACKEND_URL:", "").Trim();

                        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"✅ URL capturada via stdout: {url}");
                            return url;
                        }
                    }
                }
                return null;
            });

            // Espera discovery JSON ou stdout
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
                            Console.WriteLine($"✅ URL descoberta via JSON: {discoveredUrl}");
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

            // Se não descobriu, tenta aguardar stdout
            if (discoveredUrl == null && !stdoutTask.IsCompleted)
            {
                discoveredUrl = stdoutTask.Wait(5000) ? stdoutTask.Result : null;
            }

            if (discoveredUrl == null)
            {
                _backendProcess.Kill();
                throw new Exception("❌ Timeout: Porta dinâmica não foi detectada após 30 segundos.");
            }

            Console.WriteLine($"🎉 Backend iniciado com sucesso: {discoveredUrl}");

            return discoveredUrl;
        }
    }

    /// <summary>
    /// Encerra o backend gracefully (kill switch)
    /// </summary>
    public static async Task ShutdownBackend()
    {
        lock (_lock)
        {
            if (_backendProcess == null || !_ownedByThisProcess)
            {
                Console.WriteLine("ℹ️ Backend não foi iniciado por este processo, não será encerrado.");
                return;
            }

            if (_backendProcess.HasExited)
            {
                Console.WriteLine("ℹ️ Backend já foi encerrado.");
                return;
            }

            Console.WriteLine($"🛑 Encerrando backend (PID: {_backendProcess.Id})...");
        }

        // ============================================================================
        // 🔥 TENTATIVA 1: GRACEFUL SHUTDOWN VIA API
        // ============================================================================
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
                        Console.WriteLine("✅ Shutdown graceful enviado ao backend");

                        // Aguarda até 5 segundos para o processo encerrar
                        shutdownSuccess = _backendProcess!.WaitForExit(5000);

                        if (shutdownSuccess)
                        {
                            Console.WriteLine("✅ Backend encerrado gracefully");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao tentar shutdown graceful: {ex.Message}");
        }

        // ============================================================================
        // 🔥 TENTATIVA 2: FORCE KILL SE GRACEFUL FALHAR
        // ============================================================================
        if (!shutdownSuccess && _backendProcess != null && !_backendProcess.HasExited)
        {
            try
            {
                Console.WriteLine("⚠️ Forçando encerramento do backend...");
                _backendProcess.Kill();
                _backendProcess.WaitForExit(2000);
                Console.WriteLine("✅ Backend encerrado forçadamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao forçar encerramento: {ex.Message}");
            }
        }

        _backendProcess?.Dispose();
        _backendProcess = null;
        _ownedByThisProcess = false;
    }

    /// <summary>
    /// Verifica se um processo está vivo
    /// </summary>
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

    /// <summary>
    /// Testa se o backend está respondendo
    /// </summary>
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