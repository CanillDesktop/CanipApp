using Shared.DTOs;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Frontend.Models
{
    public class MedicamentosFiltroModel
    {
        [Display(Name = "Código")]
        public string IdMedicamento { get; set; } = string.Empty;

        [Display(Name = "Nome Comercial")]
        public string NomeComercial { get; set; } = string.Empty;

        [Display(Name = "Princípio Ativo")]
        public string PrincipioAtivo { get; set; } = string.Empty;

        [Display(Name = "Data de Fabricação")]
        public string DataFabricacao { get; set; } = string.Empty;

        [Display(Name = "Data de Validade")]
        public string DataValidade { get; set; } = string.Empty;

        [Display(Name = "Número do Lote")]
        public string NumeroLote { get; set; } = string.Empty;

        [Display(Name = "Fabricante")]
        public string Fabricante { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public static implicit operator MedicamentosFiltroDTO(MedicamentosFiltroModel model)
        {
            return new MedicamentosFiltroDTO()
            {
                IdMedicamento = model.IdMedicamento,
                NomeComercial = model.NomeComercial,
                PrincipioAtivo = model.PrincipioAtivo,
                NumeroLote = model.NumeroLote,
                Fabricante = model.Fabricante,
                Categoria = (int)Enum.Parse(typeof(CategoriaEnum), model.Categoria),
                DataFabricacao = DateTime.TryParseExact(model.DataFabricacao,"dd/MM/yyyy",CultureInfo.CurrentCulture,DateTimeStyles.None,out DateTime fabricacao) ? fabricacao : null,
                DataValidade = DateTime.TryParseExact(model.DataValidade, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime validade) ? validade.ToString("yyyy-MM-dd") : string.Empty
            };
        }
    }
}
