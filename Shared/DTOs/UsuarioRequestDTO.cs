using Shared.Enums;

namespace Shared.DTOs
{
    public class UsuarioRequestDTO
    {
        public UsuarioRequestDTO() { }

        public string? PrimeiroNome { get; set; } = string.Empty;
        public string? Sobrenome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Senha { get; set; } = string.Empty;
        public PermissoesEnum? Permissao { get; set; }
    }
}
