using Shared.DTOs;

namespace Frontend.Models
{
    internal class LoginResponseModel
    {
        public string? Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set;} = string.Empty;
        public UsuarioResponseDTO? Usuario { get; set; }

    }
}
