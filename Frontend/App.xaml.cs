namespace Frontend;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = !string.IsNullOrEmpty(StartupDiagnostics.BackendFailureUserMessage)
            ? new StartupFailurePage()
            : new MainPage();
    }
}
