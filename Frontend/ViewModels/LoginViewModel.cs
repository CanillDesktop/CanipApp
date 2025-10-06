using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components; // Necessário para NavigationManager
using Shared.Models;
using System.Net.Http.Json;

namespace Frontend.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _httpClient;

    // Melhor não usar [ObservableProperty] para AOT (Android/iOS) -> implementado manualmente
    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    // Comandos expostos explicitamente
    public IAsyncRelayCommand LoginCommand { get; }
    public IRelayCommand RegisterCommand { get; }

    public LoginViewModel(NavigationManager navigationManager, HttpClient httpClient)
    {
        _navigationManager = navigationManager;
        _httpClient = httpClient;

        LoginCommand = new AsyncRelayCommand(LoginAsync);
        RegisterCommand = new RelayCommand(Register);
    }

    private async Task LoginAsync()
    {
        var request = new
        {
            Login,
            Senha = Password
        };

        var response = await _httpClient.PostAsJsonAsync("api/login", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponseModel>();

            await SecureStorage.SetAsync("auth_token", result!.Token!.AccessToken!);
            await SecureStorage.SetAsync("refresh_token", result!.Token!.RefreshToken!);

            Preferences.Set("user_email", result.Usuario!.Email);
            Preferences.Set("user_fullname", result.Usuario.NomeCompleto());
            Preferences.Set("user_role", result.Usuario.Permissao.ToString());

            _navigationManager.NavigateTo("/home");
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            await Application.Current!.MainPage!.DisplayAlert(error!.Title, error!.Message, "OK");
        }
    }

    private void Register()
    {
        _navigationManager.NavigateTo("/cadastro");
    }
}
