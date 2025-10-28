using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Frontend.Models;

public class ProdutosFiltroModel
{
    [Display(Name = "Código")]
    public string IdProduto { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    public string DescricaoSimples { get; set; } = string.Empty;

    [Display(Name = "Data de Entrega")]
    public string DataEntrega { get; set; } = string.Empty;

    [Display(Name = "NFe")]
    public string NFe { get; set; } = string.Empty;

    public string Categoria { get; set;} = string.Empty;

    [Display(Name = "Data de Validade")]
    public string DataValidade { get; set;} = string.Empty;


    public static implicit operator ProdutosFiltroDTO(ProdutosFiltroModel model)
    {
        return new ProdutosFiltroDTO()
        { 
            IdProduto = model.IdProduto,
            DescricaoSimples = model.DescricaoSimples,
            NFe = model.NFe,
            Categoria = (int)Enum.Parse(typeof(CategoriaEnum), model.Categoria),
            DataEntrega = DateTime.ParseExact(model.DataEntrega, "dd/MM/yyyy", CultureInfo.CurrentCulture),
            DataValidade = DateTime.TryParseExact(model.DataValidade, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result) ? result.ToString() : string.Empty
        };
    }
}
