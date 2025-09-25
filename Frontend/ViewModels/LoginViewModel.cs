using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components; // Necessário para NavigationManager
using Microsoft.Maui.Controls;

namespace Frontend.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly NavigationManager _navigationManager;

    // Melhor não usar [ObservableProperty] para AOT (Android/iOS) -> implementado manualmente
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    // Comandos expostos explicitamente
    public IAsyncRelayCommand LoginCommand { get; }
    public IAsyncRelayCommand RegisterCommand { get; }

    public LoginViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;

        // Inicialização dos comandos
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        RegisterCommand = new AsyncRelayCommand(RegisterAsync);
    }

    private async Task LoginAsync()
    {
        if (!string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            Username.Equals("admin") &&
            Password.Equals("123"))
        {
            _navigationManager.NavigateTo("/home");
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert("Erro de Login", "Usuário ou senha inválidos. Tente novamente.", "OK");
        }
    }

    private async Task RegisterAsync()
    {
        await Application.Current!.MainPage!.DisplayAlert("Registrar", "A tela de registro de novos usuários seria aberta aqui.", "OK");
    }
}
