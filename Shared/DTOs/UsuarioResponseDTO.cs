using Shared.Enums;

namespace Shared.DTOs;

public class UsuarioResponseDTO
{
    public int? Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public PermissoesEnum Permissao { get; set; }
    public string? CognitoSub { get; set; }

    public string NomeCompleto() => $"{Nome} {Sobrenome}".Trim();
}