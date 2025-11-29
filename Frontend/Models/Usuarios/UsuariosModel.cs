using CommunityToolkit.Mvvm.ComponentModel;
using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Usuarios
{
    public partial class UsuariosModel : ObservableObject
    {
        private string? _primeiroNome;
        private string? _sobrenome;
        private string? _email;
        private string? _senha;
        private string? _senhaConfirmacao;
        private int _permissao;

        [Required(ErrorMessage = "O campo 'Nome' é obrigatório")]
        public string? PrimeiroNome 
        {
            get => _primeiroNome;
            set
            {
                SetProperty(ref _primeiroNome, value);
            }
        }
        public string? Sobrenome 
        {
            get => _sobrenome;
            set
            {
                SetProperty(ref _sobrenome, value);
            }
        }
        [Required(ErrorMessage = "O campo 'Email' é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email 
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
            }
        }
        [Required(ErrorMessage = "O campo 'Senha' é obrigatório")]
        public string? Senha 
        {
            get => _senha;
            set
            {
                SetProperty(ref _senha, value);
            }
        }
        [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem")]
        public string? SenhaConfirmacao 
        {
            get => _senhaConfirmacao;
            set
            {
                SetProperty(ref _senhaConfirmacao, value);
            }
        }
        [Required(ErrorMessage = "Por favor, defina um tipo de permissão")]
        public int Permissao 
        {
            get => _permissao;
            set
            {
                SetProperty(ref _permissao, value);
            }
        }

        public static implicit operator UsuarioRequestDTO(UsuariosModel model)
        {
            return new UsuarioRequestDTO
            {
                Nome = model.PrimeiroNome,
                Sobrenome = model.Sobrenome,
                Email = model.Email,
                Senha = model.Senha,
                Permissao = Enum.IsDefined(typeof(PermissoesEnum), model.Permissao) ? (PermissoesEnum)model.Permissao : PermissoesEnum.LEITURA
            };
        }
    }
}
