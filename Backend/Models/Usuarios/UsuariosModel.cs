using Amazon.DynamoDBv2.DataModel;
using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models.Usuarios
{
    [Table("Usuarios")]
    [DynamoDBTable("Usuarios")]
    public class UsuariosModel
    {
        public UsuariosModel() { }

        public UsuariosModel(string? primeiroNome, string? sobrenome, string email, string? hashSenha, PermissoesEnum? permissao)
        {
            PrimeiroNome = primeiroNome;
            Sobrenome = sobrenome;
            Email = email;
            HashSenha = hashSenha;
            Permissao = permissao;
            DataHoraCriacao = DateTime.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DynamoDBHashKey("Id")]
        public int Id { get; set; }

        [DynamoDBProperty("PrimeiroNome")]
        public string? PrimeiroNome { get; set; }

        [DynamoDBProperty("Sobrenome")]
        public string? Sobrenome { get; set; }

        [Required]
        [EmailAddress]
        [DynamoDBProperty("Email")]
        public string Email { get; set; } = string.Empty;

        [DynamoDBProperty("HashSenha")]
        public string? HashSenha { get; set; }

        [EnumDataType(typeof(PermissoesEnum))]
        [DynamoDBProperty("Permissao")]
        public PermissoesEnum? Permissao { get; set; }

        [JsonIgnore]
        [DynamoDBProperty("RefreshToken")]
        public string? RefreshToken { get; set; } = string.Empty;

        [JsonIgnore]
        [DynamoDBProperty("DataHoraExpiracaoRefreshToken")]
        public DateTime DataHoraExpiracaoRefreshToken { get; set; }

        [DynamoDBProperty("DataHoraCriacao")]
        public DateTime DataHoraCriacao { get; init; } = DateTime.UtcNow;

        [DynamoDBProperty("DataAtualizacao")]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        // ============================================================================
        // 🔥 CONVERSÃO UsuariosModel → UsuarioResponseDTO
        // ============================================================================
        public UsuarioResponseDTO ToDTO()
        {
            return new UsuarioResponseDTO
            {
                Id = this.Id,
                Email = this.Email,
                Nome = this.PrimeiroNome ?? string.Empty,
                Sobrenome = this.Sobrenome ?? string.Empty,
                Permissao = this.Permissao ?? PermissoesEnum.LEITURA,
                CognitoSub = null
            };
        }

        // ============================================================================
        // 🔥 CONVERSÃO UsuarioRequestDTO → UsuariosModel
        // ============================================================================
        public static UsuariosModel FromDTO(UsuarioRequestDTO dto)
        {
            return new UsuariosModel
            {
                Id = dto.Id ?? 0,
                PrimeiroNome = dto.Nome,
                Sobrenome = dto.Sobrenome,
                Email = dto.Email,
                HashSenha = dto.Senha,
                Permissao = dto.Permissao,
                DataAtualizacao = DateTime.UtcNow
            };
        }

        // ============================================================================
        // 🔥 OPERADORES IMPLÍCITOS
        // ============================================================================
        public static implicit operator UsuarioResponseDTO?(UsuariosModel? model)
        {
            return model?.ToDTO();
        }
    }
}