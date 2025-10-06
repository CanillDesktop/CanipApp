using Shared.Enums;

namespace Shared.DTOs
{
    public class UsuarioRequestDTO
    {
        public UsuarioRequestDTO() { }

        public int Id { get; set; }
        public string? PrimeiroNome { get; set; } = string.Empty;
        public string? Sobrenome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Senha { get; set; } = string.Empty;
        public PermissoesEnum? Permissao { get; set; }

        public static implicit operator UsuarioResponseDTO(UsuarioRequestDTO req)
        {
            return new UsuarioResponseDTO()
            {
                Id = req.Id,
                PrimeiroNome = req.PrimeiroNome,
                Sobrenome = req.Sobrenome,
                Email = req.Email,
                Permissao = req.Permissao
            };
        }
    }
}
