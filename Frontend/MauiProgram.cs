using Frontend.Diagnostics;
using Frontend.Services;
using Frontend.ViewModels;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend;

public static class MauiProgram
{
    private static readonly ILogger s_log = AppHostLogging.Create(nameof(MauiProgram));

    public static MauiApp CreateMauiApp()
    {
        s_log.LogInformation("Inicializando o host da aplicação.");

        string backendUrl;
        try
        {
            backendUrl = BackendStarter.StartBackendAndGetUrl();
            s_log.LogInformation("API local pronta. URL base: {BackendUrl}", backendUrl);
            StartupDiagnostics.BackendFailureUserMessage = null;
            StartupDiagnostics.BackendFailureTechnicalSummary = null;
        }
        catch (Exception ex)
        {
            s_log.LogError(ex, "Falha ao iniciar ou descobrir a API local.");
            StartupDiagnostics.BackendFailureUserMessage =
                "O aplicativo depende de um serviço local (API) que não pôde ser iniciado. Sem ele, login e dados não funcionam.";
            StartupDiagnostics.BackendFailureTechnicalSummary = ex.Message;
            backendUrl = "http://127.0.0.1:1";
        }

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

        builder.Services.AddSingleton(new BackendConfig { Url = backendUrl });

        builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);

        builder.Services.AddScoped<AuthenticationStateService>();

        builder.Services.AddScoped<CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthenticationStateProvider>());

        builder.Services.AddAuthorizationCore();

        builder.Services.AddTransient<AuthDelegatingHandler>();

        builder.Services.AddScoped<ProdutosViewModel>();
        builder.Services.AddScoped<MedicamentosViewModel>();
        builder.Services.AddScoped<InsumosViewModel>();
        builder.Services.AddScoped<LoginViewModel>();
        builder.Services.AddScoped<CadastroViewModel>();
        builder.Services.AddScoped<EstoqueDetailViewModel>();
        builder.Services.AddScoped<AddLoteEstoqueViewModel>();

        builder.Services.AddHttpClient("ApiClient", (sp, client) =>
        {
            var cfg = sp.GetRequiredService<BackendConfig>();
            client.BaseAddress = new Uri(cfg.Url);
            client.Timeout = TimeSpan.FromMinutes(4);

            AppHostLogging.Create("HttpClient.ApiClient")
                .LogInformation("HttpClient configurado com BaseAddress: {BaseAddress}", cfg.Url);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
        .AddHttpMessageHandler<AuthDelegatingHandler>();

        var app = builder.Build();

        RegisterKillSwitch();

        s_log.LogInformation("Aplicação MAUI construída com sucesso.");

        return app;
    }

    /// <summary>
    /// Garante que o processo da API embutida seja encerrado ao sair (hooks de encerramento).
    /// </summary>
    private static void RegisterKillSwitch()
    {
        var killSwitchLog = AppHostLogging.Create("KillSwitch");

        try
        {
            AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
            {
                killSwitchLog.LogInformation("Processo encerrando; parando a API local se pertencer a esta sessão.");
                try
                {
                    await BackendStarter.ShutdownBackend();
                }
                catch (Exception ex)
                {
                    killSwitchLog.LogWarning(ex, "Falha no hook de encerramento durante ProcessExit.");
                }
            };

            AppDomain.CurrentDomain.UnhandledException += (_, _) =>
            {
                killSwitchLog.LogWarning("Exceção não tratada; tentando parar a API local.");
                try
                {
                    var task = BackendStarter.ShutdownBackend();
                    task.Wait(TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
                    killSwitchLog.LogWarning(ex, "Falha no hook de encerramento após exceção não tratada.");
                }
            };

#if WINDOWS
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("CloseWindow", (handler, view) =>
            {
                var nativeWindow = handler.PlatformView;

                nativeWindow.Closed += async (_, _) =>
                {
                    killSwitchLog.LogInformation("Janela principal fechada; parando a API local quando aplicável.");
                    try
                    {
                        await BackendStarter.ShutdownBackend();
                    }
                    catch (Exception ex)
                    {
                        killSwitchLog.LogWarning(ex, "Falha no hook de encerramento ao fechar a janela.");
                    }
                };
            });
#endif

            System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += async (_) =>
            {
                killSwitchLog.LogInformation("Descarregando contexto de assembly; parando a API local quando aplicável.");
                try
                {
                    await BackendStarter.ShutdownBackend();
                }
                catch (Exception ex)
                {
                    killSwitchLog.LogWarning(ex, "Falha no hook de encerramento durante descarga de assembly.");
                }
            };

            killSwitchLog.LogDebug("Hooks de saída da aplicação registrados.");
        }
        catch (Exception ex)
        {
            killSwitchLog.LogWarning(ex, "Não foi possível registrar todos os hooks de saída.");
        }
    }
}
