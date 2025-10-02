using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models.Usuarios
{
    public class UsuariosModel
    {
        public UsuariosModel(string? primeiroNome, string? sobrenome, string email, string? hashSenha, PermissoesEnum? permissao)
        {
            Id = new Random().Next(1000);
            PrimeiroNome = primeiroNome;
            Sobrenome = sobrenome;
            Email = email;
            HashSenha = hashSenha;
            Permissao = permissao;
            DataHoraCriacao = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }
        public string? PrimeiroNome { get; set; }
        public string? Sobrenome { get; set; }
        public string Email { get; set; }
        public string? HashSenha { get; set; }

        [EnumDataType(typeof(PermissoesEnum))]
        public PermissoesEnum? Permissao {  get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; } = string.Empty;

        [JsonIgnore]
        public DateTime DataHoraExpiracaoRefreshToken { get; set; }

        [JsonIgnore]
        public DateTime DataHoraCriacao { get; init; }

        public static implicit operator UsuariosModel(UsuarioRequestDTO dto)
        {
            return new UsuariosModel(
                dto.PrimeiroNome,
                dto.Sobrenome,
                dto.Email,
                dto.Senha,
                dto.Permissao
            );
        }

        public static implicit operator UsuarioResponseDTO(UsuariosModel model)
        {
            return new UsuarioResponseDTO()
            {
                Id = model.Id,
                PrimeiroNome = model.PrimeiroNome,
                Sobrenome = model.Sobrenome,
                Email = model.Email,
                Permissao = model.Permissao
            };
        }
    }
}
