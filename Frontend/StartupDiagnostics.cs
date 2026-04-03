namespace Frontend;

/// <summary>
/// Estado definido durante <see cref="MauiProgram.CreateMauiApp"/> quando o Backend local não pôde ser iniciado,
/// para que <see cref="App"/> exiba uma tela explicativa em vez de encerrar sem feedback visual.
/// </summary>
public static class StartupDiagnostics
{
    public static string? BackendFailureUserMessage { get; set; }

    public static string? BackendFailureTechnicalSummary { get; set; }

    public static string LogsDirectory =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CanilApp",
            "logs");
}
