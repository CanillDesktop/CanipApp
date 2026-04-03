namespace Frontend;

public partial class MainPage : ContentPage
{
    /// <summary>
    /// Ponha a <c>true</c> para substituir temporariamente a tela por um WebView do MAUI só com URL HTTPS (teste do motor).
    /// Não pode ser <c>const</c>: senão o compilador marcaria o bloco como inalcançável e avisaria.
    /// </summary>
    private static bool DiagnosticoSomenteWebView2Externo = false;

#if DEBUG && WINDOWS
    private bool _ferramentasDesenvolvedorJaAbertas;
#endif

    public MainPage()
    {
        if (DiagnosticoSomenteWebView2Externo)
        {
#if WINDOWS
            Content = new WebView
            {
                Source = new UrlWebViewSource { Url = "https://www.google.com" },
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            return;
#endif
        }

        InitializeComponent();

#if DEBUG && WINDOWS
        blazorWebView.HandlerChanged += OnBlazorWebViewHandlerChangedDiagnostic;
#endif
    }

#if DEBUG && WINDOWS
    private void OnBlazorWebViewHandlerChangedDiagnostic(object? sender, EventArgs e)
    {
        if (_ferramentasDesenvolvedorJaAbertas)
            return;

        try
        {
            if (blazorWebView.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.WebView2 wv2)
                return;

            _ferramentasDesenvolvedorJaAbertas = true;
            _ = AbrirFerramentasDesenvolvedorQuandoProntoAsync(wv2);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Diagnóstico WebView2] {ex}");
        }
    }

    private static async Task AbrirFerramentasDesenvolvedorQuandoProntoAsync(Microsoft.UI.Xaml.Controls.WebView2 wv2)
    {
        try
        {
            await wv2.EnsureCoreWebView2Async();
            wv2.CoreWebView2.OpenDevToolsWindow();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Diagnóstico WebView2] EnsureCoreWebView2 / DevTools: {ex}");
        }
    }
#endif
}
