using Shared.DTOs;

namespace Shared.Models
{
    public class LoginResponseModel
    {
        public TokenResponse? Token { get; set; }
        public UsuarioResponseDTO? Usuario { get; set; }

    }
}
