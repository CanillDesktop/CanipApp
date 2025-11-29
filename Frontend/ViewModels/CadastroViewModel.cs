using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.DTOs;
using Shared.Enums;
using System.Net.Http.Json;

namespace Frontend.ViewModels
{
    public class CadastroViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;

        private string _primeiroNome = string.Empty;
        public string PrimeiroNome
        {
            get => _primeiroNome;
            set => SetProperty(ref _primeiroNome, value);
        }

        private string _sobrenome = string.Empty;
        public string Sobrenome
        {
            get => _sobrenome;
            set => SetProperty(ref _sobrenome, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _senha = string.Empty;
        public string Senha
        {
            get => _senha;
            set => SetProperty(ref _senha, value);
        }

        private string _senhaConfirmacao = string.Empty;
        public string SenhaConfirmacao
        {
            get => _senhaConfirmacao;
            set => SetProperty(ref _senhaConfirmacao, value);
        }

        private string _permissaoSelecionada = "LEITURA";
        public string PermissaoSelecionada
        {
            get => _permissaoSelecionada;
            set => SetProperty(ref _permissaoSelecionada, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string? _successMessage;
        public string? SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public List<string> Permissoes { get; } = new()
        {
            "LEITURA",
            "ESCRITA",
            "ADMIN"
        };

        public event Action<string>? NavigationRequested;

        public CadastroViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            RegistrarCommand = new AsyncRelayCommand(RegistrarAsync);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        }

        public IAsyncRelayCommand RegistrarCommand { get; }
        public IRelayCommand NavigateToLoginCommand { get; }

        private async Task RegistrarAsync()
        {
            ErrorMessage = null;
            SuccessMessage = null;

            if (string.IsNullOrWhiteSpace(PrimeiroNome))
            {
                ErrorMessage = "Nome é obrigatório";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email é obrigatório";
                return;
            }

            if (string.IsNullOrWhiteSpace(Senha) || Senha.Length < 8)
            {
                ErrorMessage = "Senha deve ter no mínimo 8 caracteres";
                return;
            }

            if (Senha != SenhaConfirmacao)
            {
                ErrorMessage = "As senhas não coincidem";
                return;
            }

            if (!ValidarSenhaForte(Senha))
            {
                ErrorMessage = "Senha deve conter: maiúscula, minúscula, número e caractere especial";
                return;
            }

            IsLoading = true;

            try
            {
                var request = new UsuarioRequestDTO
                {
                    Nome = PrimeiroNome,
                    Sobrenome = Sobrenome,
                    Email = Email,
                    Senha = Senha,
                    Permissao = Enum.Parse<PermissoesEnum>(PermissaoSelecionada)
                };

                var response = await _httpClient.PostAsJsonAsync("api/usuarios", request);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Cadastro realizado! Redirecionando para login...";
                    await Task.Delay(2000);
                    NavigationRequested?.Invoke("/login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Falha no cadastro: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateToLogin()
        {
            NavigationRequested?.Invoke("/login");
        }

        private bool ValidarSenhaForte(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha)) return false;

            bool temMaiuscula = senha.Any(char.IsUpper);
            bool temMinuscula = senha.Any(char.IsLower);
            bool temNumero = senha.Any(char.IsDigit);
            bool temEspecial = senha.Any(c => !char.IsLetterOrDigit(c));

            return temMaiuscula && temMinuscula && temNumero && temEspecial;
        }
    }
}