namespace Frontend;



// ": ContentPage": significa que a LoginPage "herda" todas as características e funcionalidades de uma página padrão do .NET MAUI.
public partial class LoginPage : ContentPage
{
    
    public LoginPage()
    {
        // Este comando é crucial. Ele lê o arquivo LoginPage.xaml,
        // cria todos os elementos visuais (botões, labels, etc.) e os conecta
        // a este arquivo de código C#.
        InitializeComponent();
    }

    // Este é o método que é executado quando o botão de Login é clicado.
    
    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        
        string username = UsernameEntry.Text;
        
        string password = PasswordEntry.Text;

        
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) && username.Equals("admin") && password.Equals("123"))
        {
           
            await Shell.Current.GoToAsync("//MainPage");
        }
        else 
        {
            
            await DisplayAlert("Erro de Login", "Usuário ou senha inválidos. Tente novamente.", "OK");
        }
    }

    
    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        
        await DisplayAlert("Registrar", "A tela de registro de novos usuários seria aberta aqui.", "OK");
    }
}