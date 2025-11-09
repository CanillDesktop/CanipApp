using Amazon.DynamoDBv2.DataModel;
using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models.Produtos
{

    [DynamoDBTable("Produtos")]
    public class Produtos
    {
        private string? _validade;

        public Produtos() { }

        [Key]
        [DynamoDBHashKey("id")]
       
        public string IdProduto { get; set; } = string.Empty;
        public string? DescricaoSimples { get; set; }
        public DateTime? DataEntrega { get; init; }

        public string? NFe { get; set; }
        public string? DescricaoDetalhada { get; set; }
        public UnidadeEnum Unidade { get; set; }
        public CategoriaEnum Categoria { get; set; }

        public bool IsDeleted { get; set; } = false;
        public int Quantidade { get; set; }
        [DynamoDBProperty]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public string? Validade
        {
            get => _validade;
            set
            {
                if (DateTime.TryParse(value, out var _))
                    _validade = value;
            }
        }
 
        public DateTime DataHoraInsercaoRegistro { get; set; }
        public int EstoqueDisponivel { get; set; }




        public static implicit operator Produtos(ProdutosDTO dto)
        {
            return new Produtos()
            {
                IdProduto = dto.IdProduto,
                DescricaoSimples = dto.DescricaoSimples,
                DataEntrega = dto.DataEntrega,
                NFe = dto.NFe,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,
                Categoria = dto.Categoria,
                Quantidade = dto.Quantidade,
                Validade = dto.Validade,
                EstoqueDisponivel = dto.EstoqueDisponivel
            };
        }

        public static implicit operator ProdutosDTO(Produtos model)
        {
            return new ProdutosDTO()
            {
                IdProduto = model.IdProduto,
                DescricaoSimples = model.DescricaoSimples,
                DataEntrega = model.DataEntrega,
                NFe = model.NFe,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Unidade = model.Unidade,
                Categoria = model.Categoria,
                Quantidade = model.Quantidade,
                Validade = model.Validade,
                EstoqueDisponivel = model.EstoqueDisponivel
            };
        }
    }
}
