using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Models.Medicamentos
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicamentosController : ControllerBase
    {
        public enum PrioridadeEnum
        {

            Baixa,
            Media,
            Alta
        }


        public PrioridadeEnum Prioridade { get; set; }




        public String DescricaoMedicamentos { get; set; }



        public DateTime DataEntrega { get; set; }




        public string NotaFiscal { get; set; }

        public string NomeComercial { get; set; }
        public string HorV { get; set; }// Esse perguntar pra eles oq significa para melhorar o nome
        public int ConsumoMensal { get; set; }

        public int ConsumoAnual { get; set; }

        public DateTime Validade { get; set; }

        public int EstoqueDisponivel { get; set; }

        public int EntradaEstoque {  get; set; }

        public int SaidaTotalEstoque {  get; set; }



    }
}



