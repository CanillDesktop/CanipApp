using Frontend.Handlers;
using Frontend.Services; // ✅ Adicionado
using Frontend.ViewModels;
using Microsoft.Extensions.Logging;

namespace Frontend;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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

        builder.Services.AddScoped<ProdutosViewModel>();
        builder.Services.AddScoped<MedicamentosViewModel>();
        builder.Services.AddScoped<LoginViewModel>();
        builder.Services.AddScoped<CadastroViewModel>();

        // 🟩 LINHAS NOVAS - necessárias para o painel de controle
        builder.Services.AddScoped<ServicoDeEstoque>();
        builder.Services.AddScoped<DashboardViewModel>();
        // 🟩 FIM DAS LINHAS NOVAS

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7019/")
        });
        builder.Services.AddTransient<AuthDelegatingHandler>();
        builder.Services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7019");
        })
        .AddHttpMessageHandler<AuthDelegatingHandler>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
