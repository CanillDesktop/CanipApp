using Shared.DTOs; // Assumindo que InsumosFiltroDTO ficará aqui
using Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Frontend.Models
{
    public class InsumosFiltroModel
    {
        [Display(Name = "Código")]
        public string CodigoId { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string DescricaoSimplificada { get; set; } = string.Empty;

        [Display(Name = "Data de Entrada")]
        public string DataEntrada { get; set; } = string.Empty;

        [Display(Name = "NFe")]
        public string NotaFiscal { get; set; } = string.Empty;

        [Display(Name = "Unidade")]
        public string Unidade { get; set; } = string.Empty;

        [Display(Name = "Data de Validade")]
        public string DataValidade { get; set; } = string.Empty;

        // Assumindo que um InsumosFiltroDTO exista em Shared.DTOs
        // Se não, você precisará criá-lo.
        public static implicit operator InsumosFiltroDTO(InsumosFiltroModel model)
        {
            // Tenta converter o Enum 'Unidade' para seu valor int
            int? unidadeValor = null;
            if (!string.IsNullOrEmpty(model.Unidade) && Enum.TryParse(typeof(UnidadeInsumosEnum), model.Unidade, true, out var unidadeObj))
            {
                unidadeValor = (int)unidadeObj;
            }

            return new InsumosFiltroDTO()
            {
                // Converte string para int?, tratando string vazia
                CodigoId = int.TryParse(model.CodigoId, out int codigoId) ? codigoId : null,

                DescricaoSimplificada = model.DescricaoSimplificada,

               NotaFiscal = model.NotaFiscal,

                Unidade = unidadeValor,

                // Converte a string de data "dd/MM/yyyy" para DateTime?
                DataEntrada = DateTime.TryParseExact(model.DataEntrada, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime dataEntrada)
                              ? dataEntrada
                              : null,

                // Converte a string de data "dd/MM/yyyy" para DateOnly? (que é o tipo do InsumosModel)
                DataValidade = DateOnly.TryParseExact(model.DataValidade, "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateOnly dataValidade)
                               ? dataValidade
                               : null
            };
        }
    }
}

/* --- NOTA ---
Você precisará adicionar a classe 'InsumosFiltroDTO' ao seu projeto 'Shared',
provavelmente no namespace 'Shared.DTOs', da seguinte forma:

namespace Shared.DTOs
{
    public class InsumosFiltroDTO
    {
        public int? CodigoId { get; set; }
        public string? DescricaoSimplificada { get; set; }
        public DateTime? DataEntrada { get; set; }
        public string? NotaFiscal { get; set; }
        public int? Unidade { get; set; } // Enviando o valor int do Enum
        public DateOnly? DataValidade { get; set; }
    }
}
*/