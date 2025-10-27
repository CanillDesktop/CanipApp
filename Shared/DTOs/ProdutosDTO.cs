using Shared.Enums;
using Shared.Interfaces; // Para integração com o painel de controle
using System;
using System.Text.Json.Serialization;

namespace Shared.DTOs
{
    public class ProdutosDTO : IEstoqueItem
    {
        public ProdutosDTO() { }
        public ProdutosDTO(string? descricaoSimples, DateTime? dataEntrega, string? nFe, string? descricaoDetalhada, UnidadeEnum unidade, CategoriaEnum categoria,
            int quantidade = 0, string? validade = null, int estoqueDisponivel = 0)
        {
            IdProduto = GerarIdentificador(categoria);
            DescricaoSimples = descricaoSimples;
            DataEntrega = dataEntrega;
            NFe = nFe;
            DescricaoDetalhada = descricaoDetalhada;
            Unidade = unidade;
            Categoria = categoria;
            Quantidade = quantidade;
            Validade = validade;
            EstoqueDisponivel = estoqueDisponivel;
            NivelMinimoEstoque = nivelMinimo;
        }

        public string IdProduto { get; set; } = string.Empty;
        public string? DescricaoSimples { get; set; }
        public DateTime? DataEntrega { get; init; }
        public string? NFe { get; set; }
        public string? DescricaoDetalhada { get; set; }
        public UnidadeEnum Unidade { get; set; }
        public CategoriaEnum Categoria { get; set; }
        public int Quantidade { get; set; }
        public string? Validade { get; set; }
        public int EstoqueDisponivel { get; set; }
        public int NivelMinimoEstoque { get; set; }

        // ==============================
        // IMPLEMENTAÇÃO DO IEstoqueItem
        // ==============================

        [JsonIgnore]
        public string Nome => DescricaoDetalhada ?? DescricaoSimples ?? "Produto sem nome";

        [JsonIgnore]
        public int QuantidadeAtual => EstoqueDisponivel;

        [JsonIgnore]
        public DateTime? DataDeValidade
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Validade))
                    return null;

                if (DateTime.TryParseExact(
                    Validade,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime data))
                {
                    return data;
                }

                return null;
            }
        }

        // ===========================
        // GERADOR DE IDENTIFICADOR
        // ===========================
        private static string GerarIdentificador(CategoriaEnum categoria)
        {
            string id = string.Empty;
            string catString = categoria.ToString();

            if (catString.Length < 3)
                id += catString;
            else
                id += catString[..3];

            if (catString.Contains('_'))
            {
                int i = catString.IndexOf('_');
                id += catString.Substring(i + 1, 1);
            }

            id += Guid.NewGuid().ToString().Replace("-", "");
            return id;
        }
    }
}
