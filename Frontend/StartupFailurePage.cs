using System.Text;

namespace Frontend;

/// <summary>
/// Tela mínima (sem Blazor/API) quando o processo Backend não inicia.
/// </summary>
public class StartupFailurePage : ContentPage
{
    public StartupFailurePage()
    {
        Title = "CanilApp";

        var userText = StartupDiagnostics.BackendFailureUserMessage
            ?? "Não foi possível iniciar o serviço local da aplicação.";
        var technical = StartupDiagnostics.BackendFailureTechnicalSummary;
        var logs = StartupDiagnostics.LogsDirectory;

        var body = new StringBuilder();
        body.AppendLine(userText);
        if (!string.IsNullOrWhiteSpace(technical))
        {
            body.AppendLine();
            body.AppendLine("Detalhe técnico:");
            body.AppendLine(technical);
        }

        var closeBtn = new Button
        {
            Text = "Fechar aplicativo",
            Margin = new Thickness(0, 16, 0, 0)
        };
        closeBtn.Clicked += (_, _) =>
        {
            if (Application.Current != null)
                Application.Current.Quit();
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Não foi possível iniciar o serviço local",
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = body.ToString(),
                        FontSize = 14
                    },
                    new Label
                    {
                        Text = "Dicas:\n• Confira se a pasta Backend está ao lado do executável (após publicar ou compilar o projeto Frontend).\n• Se o Backend não for autocontido, instale o .NET 8 Runtime no Windows.\n• Verifique antivírus e permissões de execução.",
                        FontSize = 13,
                        TextColor = Colors.DarkGray
                    },
                    new Label
                    {
                        Text = $"Logs do serviço (quando chegar a iniciar):\n{logs}",
                        FontSize = 12,
                        TextColor = Colors.Grey
                    },
                    closeBtn
                }
            }
        };
    }
}
