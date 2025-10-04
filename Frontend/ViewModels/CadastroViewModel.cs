using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Frontend.Models;
using Microsoft.AspNetCore.Components;
using Shared.DTOs;
using Shared.Enums;
using Shared.Models;
using System.Net.Http.Json;

namespace Frontend.ViewModels;

public partial class CadastroViewModel : ObservableObject
{
    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _httpClient;

    private UsuariosModel _usuario = new();

    public UsuariosModel Usuario
    {
        get => _usuario;
        set
        {
            SetProperty(ref _usuario, value);
        }
    }

    public IAsyncRelayCommand RegisterCommand;

    public CadastroViewModel(NavigationManager navigationManager, HttpClient httpClient)
    {
        _navigationManager = navigationManager;
        _httpClient = httpClient;
        RegisterCommand = new AsyncRelayCommand(RegisterAsync);
    }

    private async Task RegisterAsync()
    {
        Usuario.Permissao = (int)PermissoesEnum.LEITURA;
        UsuarioRequestDTO dto = Usuario;

        var response = await _httpClient.PostAsJsonAsync("api/usuarios", dto);

        if (response.IsSuccessStatusCode)
        {
            await Application.Current!.MainPage!.DisplayAlert("Sucesso", "Usuário cadastrado com sucesso!", "OK");

            _navigationManager.NavigateTo("/");
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            await Application.Current!.MainPage!.DisplayAlert(error!.Title, error!.Message, "OK");
        }
    }
}
