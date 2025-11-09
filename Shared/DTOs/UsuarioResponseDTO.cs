using Shared.Enums;

namespace Shared.DTOs
{
    public class UsuarioResponseDTO
    {
        public int? Id { get; set; }
        public string? PrimeiroNome { get; set; } = string.Empty;
        public string? Sobrenome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public PermissoesEnum? Permissao { get; set; }

        public string? NomeCompleto() => $"{PrimeiroNome} {Sobrenome}";
    }
}
