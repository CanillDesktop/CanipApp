using Shared.DTOs;
using Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Produtos
{
    public class ProdutosModel
    {
        private string? _validade;

        public ProdutosModel() { }

        [Key]
        [JsonIgnore]
        public string IdProduto { get; set; } = string.Empty;

        [Display(Name = "Descrição Simples")]
        public string? DescricaoSimples { get; set; }

        [Display(Name = "Data de Entrega")]
        public DateTime? DataEntrega { get; set; } // ✅ Agora sem “init”, permite conversão sem erro

        [Display(Name = "NFe / Documento")]
        public string? NFe { get; set; }

        [Display(Name = "Descrição Detalhada")]
        public string? DescricaoDetalhada { get; set; }

        [Display(Name = "Unidade")]
        public UnidadeEnum Unidade { get; set; }

        [Display(Name = "Categoria")]
        public CategoriaEnum Categoria { get; set; }

        [Display(Name = "Quantidade")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int Quantidade { get; set; }

        [Display(Name = "Validade")]
        public string? Validade
        {
            get => _validade ?? "indeterminado";
            set
            {
                // Valida se o valor é uma data válida antes de aceitar
                if (string.IsNullOrWhiteSpace(value))
                {
                    _validade = null;
                    return;
                }

                if (DateTime.TryParse(value, out _))
                    _validade = value;
                else
                    throw new FormatException($"Formato de data inválido: '{value}'. Use dd/MM/yyyy");
            }
        }

        [Display(Name = "Data e Hora do Registro")]
        public DateTime DataHoraInsercaoRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Estoque Disponível")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int EstoqueDisponivel { get; set; }

        [Display(Name = "Nível Mínimo de Estoque")]
        [Range(0, int.MaxValue, ErrorMessage = "O valor deve ser positivo")]
        public int NivelMinimoEstoque { get; set; } = 5; // ✅ Novo campo para alertas do painel

        // ============================================================
        // Conversão automática entre Model e DTO
        // ============================================================

        public static implicit operator ProdutosModel(ProdutosDTO dto)
        {
            return new ProdutosModel
            {
                IdProduto = dto.IdProduto,
                DescricaoSimples = dto.DescricaoSimples,
                DataEntrega = dto.DataEntrega, // ✅ Agora aceita conversão de DateTime (sem erro CS0266)
                NFe = dto.NFe,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,
                Categoria = dto.Categoria,
                Quantidade = dto.Quantidade,
                Validade = dto.Validade,
                EstoqueDisponivel = dto.EstoqueDisponivel,
                NivelMinimoEstoque = dto.NivelMinimoEstoque
            };
        }

        public static implicit operator ProdutosDTO(ProdutosModel model)
        {
            return new ProdutosDTO
            {
                IdProduto = model.IdProduto,
                DescricaoSimples = model.DescricaoSimples,
                DataEntrega = model.DataEntrega ?? DateTime.Now, // ✅ Garante conversão segura
                NFe = model.NFe,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                Categoria = model.Categoria,
                Quantidade = model.Quantidade,
                Validade = model.Validade,
                EstoqueDisponivel = model.EstoqueDisponivel,
                NivelMinimoEstoque = model.NivelMinimoEstoque
            };
        }
    }
}
