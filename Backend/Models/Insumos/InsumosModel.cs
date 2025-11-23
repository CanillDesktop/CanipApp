using Shared.DTOs.Estoque;
using Shared.DTOs.Insumos;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models.Insumos
{
    [DynamoDBTable("Insumos")]
    public class InsumosModel : ItemComEstoqueBaseModel
    {
        public string CodInsumo { get; set; } = string.Empty;
        public required string DescricaoSimplificada { get; set; }
        public required string DescricaoDetalhada { get; set; }
        public UnidadeInsumosEnum Unidade { get; set; }
        public bool IsDeleted { get; set; } = false;

        [DynamoDBProperty]
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public static implicit operator InsumosModel(InsumosCadastroDTO dto)
        {
            return new InsumosModel()
            {
                CodInsumo = dto.CodInsumo,
                DescricaoSimplificada = dto.DescricaoSimplificada,
                DescricaoDetalhada = dto.DescricaoDetalhada,
                Unidade = dto.Unidade,
                ItemNivelEstoque = new()
                {
                    NivelMinimoEstoque = dto.NivelMinimoEstoque
                },
                ItensEstoque =
                [
                    new ItemEstoqueModel()
                    {
                        CodItem = dto.CodInsumo,
                        DataEntrega = dto.DataEntrega,
                        DataValidade = dto.DataValidade,
                        Lote = dto.Lote,
                        NFe = dto.NFe,
                        Quantidade = dto.Quantidade
                    }
                ]
            };
        }

        public static implicit operator InsumosCadastroDTO(InsumosModel model)
        {
            var itemEstoque = model.ItensEstoque.FirstOrDefault();
            return new InsumosCadastroDTO()
            {
                CodInsumo = model.CodInsumo,
                DescricaoSimplificada = model.DescricaoSimplificada,
                DescricaoDetalhada = model.DescricaoDetalhada,
                Lote = itemEstoque?.Lote,
                Quantidade = itemEstoque == null ? 0 : itemEstoque!.Quantidade,
                DataEntrega = itemEstoque == null ? DateTime.Now : itemEstoque.DataEntrega,
                NFe = itemEstoque?.NFe,
                NivelMinimoEstoque = model.ItemNivelEstoque.NivelMinimoEstoque,
                DataValidade = itemEstoque?.DataValidade,
            };
        }

        public static implicit operator InsumosLeituraDTO(InsumosModel model)
        {
            return new InsumosLeituraDTO()
            {
                IdItem = model.IdItem,
                CodItem = model.CodInsumo,
                NomeItem = model.DescricaoSimplificada,
                DescricaoDetalhada = model.DescricaoDetalhada,
                ItemNivelEstoque = model.ItemNivelEstoque,
                ItensEstoque = [.. model.ItensEstoque.Select(e => (ItemEstoqueDTO)e)]
            };
        }
    }
}