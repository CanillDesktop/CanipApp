using Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs;

public class UsuarioRequestDTO
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    public PermissoesEnum Permissao { get; set; } = PermissoesEnum.LEITURA;
}