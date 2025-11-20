using Shared.DTOs.Insumos;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Frontend.Models.Insumos
{
    public class InsumosFiltroModel
    {
        [Display(Name = "Código")]
        public string CodInsumo { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string DescricaoSimplificada { get; set; } = string.Empty;
        public string Unidade { get; set; } = string.Empty; 

        [Display(Name = "Data de Entrega")]
        public string DataEntrega { get; set; } = string.Empty;

        [Display(Name = "NFe")]
        public string NFe { get; set; } = string.Empty;

        [Display(Name = "Data de Validade")]
        public string DataValidade { get; set; } = string.Empty;


        public static implicit operator InsumosFiltroDTO(InsumosFiltroModel model)
        {
            return new InsumosFiltroDTO()
            {
                CodInsumo = model.CodInsumo,
                DescricaoSimplificada = model.DescricaoSimplificada,
                NFe = model.NFe,
                Unidade = (int)Enum.Parse(typeof(UnidadeInsumosEnum), model.Unidade),
                DataEntrega = DateTime.ParseExact(model.DataEntrega, "dd/MM/yyyy", CultureInfo.CurrentCulture),
                DataValidade = DateTime.ParseExact(model.DataValidade, "dd/MM/yyyy", CultureInfo.CurrentCulture)
            };
        }
    }
}
