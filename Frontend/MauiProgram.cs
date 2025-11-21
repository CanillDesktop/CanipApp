using Frontend.Services;
using Frontend.ViewModels;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Console.WriteLine("🚀 Iniciando MauiProgram...");

        // ============================================================================
        // 🔥 ETAPA 1: INICIA BACKEND E OBTÉM URL DINÂMICA
        // ============================================================================
        string backendUrl;
        try
        {
            backendUrl = BackendStarter.StartBackendAndGetUrl();
            Console.WriteLine($"✅ Backend iniciado com sucesso: {backendUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERRO CRÍTICO ao iniciar backend: {ex.Message}");
            throw new Exception($"Falha ao iniciar backend: {ex.Message}", ex);
        }

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // ============================================================================
        // 🔥 CULTURA PT-BR
        // ============================================================================
        var culture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        // Logging básico sem AddDebug
        builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

        // ============================================================================
        // 🔥 REGISTRA URL DO BACKEND NO DI (USA CLASSE EXISTENTE)
        // ============================================================================
        builder.Services.AddSingleton(new BackendConfig { Url = backendUrl });

        // ============================================================================
        // 🔥 REGISTRA DELEGATING HANDLER
        // ============================================================================
      

        // ============================================================================
        // 🔥 VIEWMODELS
        // ============================================================================
        builder.Services.AddScoped<ProdutosViewModel>();
        builder.Services.AddScoped<MedicamentosViewModel>();
        builder.Services.AddScoped<LoginViewModel>();
        builder.Services.AddScoped<CadastroViewModel>();
        builder.Services.AddScoped<InsumosViewModel>();

        // ============================================================================
        // 🔥 HTTPCLIENT COM URL DINÂMICA E AUTH HANDLER
        // ============================================================================
        builder.Services.AddHttpClient("ApiClient", (sp, client) =>
        {
            var cfg = sp.GetRequiredService<BackendConfig>();
            client.BaseAddress = new Uri(cfg.Url);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
   .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())   // obrigatório
  .AddHttpMessageHandler(() => new AuthDelegatingHandler());

        var app = builder.Build();

        // ============================================================================
        // 🔥 REGISTRA KILL SWITCH NO ENCERRAMENTO DO APP
        // ============================================================================
        RegisterKillSwitch();

        Console.WriteLine("✅ MauiApp configurado com sucesso!");

        return app;
    }

    /// <summary>
    /// Registra kill switch para encerrar backend quando o app fechar
    /// </summary>
    private static void RegisterKillSwitch()
    {
        try
        {
            // ============================================================================
            // 🔥 KILL SWITCH: ProcessExit é o evento mais confiável
            // ============================================================================
            AppDomain.CurrentDomain.ProcessExit += async (s, e) =>
            {
                Console.WriteLine("🛑 Aplicação encerrando via ProcessExit, executando kill switch...");
                try
                {
                    await BackendStarter.ShutdownBackend();
                    Console.WriteLine("✅ Kill switch executado com sucesso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro no kill switch: {ex.Message}");
                }
            };

            // ============================================================================
            // 🔥 FALLBACK: UnhandledException (app crashes)
            // ============================================================================
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Console.WriteLine("🛑 UnhandledException detectado, tentando kill switch...");
                try
                {
                    // Tentativa síncrona de shutdown
                    var task = BackendStarter.ShutdownBackend();
                    task.Wait(TimeSpan.FromSeconds(3));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro no kill switch (crash): {ex.Message}");
                }
            };

            // ============================================================================
            // 🔥 WINDOWS SPECIFIC: Window Closing Events
            // ============================================================================
#if WINDOWS
            // Hook no evento de fechamento da janela principal
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping("CloseWindow", (handler, view) =>
            {
                var nativeWindow = handler.PlatformView;

                nativeWindow.Closed += async (s, e) =>
                {
                    Console.WriteLine("🛑 Window.Closed detectado, executando kill switch...");
                    try
                    {
                        await BackendStarter.ShutdownBackend();
                        Console.WriteLine("✅ Kill switch executado (Window.Closed)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erro no kill switch (Window.Closed): {ex.Message}");
                    }
                };
            });
#endif

            // ============================================================================
            // 🔥 ASSEMBLY UNLOAD (Fallback genérico)
            // ============================================================================
            System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += async (ctx) =>
            {
                Console.WriteLine("🛑 Assembly unloading, executando kill switch...");
                try
                {
                    await BackendStarter.ShutdownBackend();
                    Console.WriteLine("✅ Kill switch executado (Assembly unload)");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro no kill switch (Assembly unload): {ex.Message}");
                }
            };

            Console.WriteLine("✅ Kill switch registrado com sucesso");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao registrar kill switch: {ex.Message}");
            // Não impede a inicialização do app
        }
    }
}