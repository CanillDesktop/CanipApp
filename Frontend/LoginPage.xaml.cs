namespace Frontend;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

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
        // Aqui você colocaria a lógica para navegar para uma nova página de registro.
        // Exemplo: await Navigation.PushAsync(new RegisterPage());

        await DisplayAlert("Registrar", "A tela de registro de novos usuários seria aberta aqui.", "OK");
    }
}