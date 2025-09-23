using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Backend.Context;
using Backend.Models.Medicamentos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MedicamentosController : Controller
    {
        private readonly CanilAppDbContext _context;

        public MedicamentosController(CanilAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<MedicamentosModel>> Get()
        {
            var medicamentos = _context.Medicamentos.AsNoTracking().ToList();

            if (medicamentos is null)
            {
                return NotFound();
            }

            return medicamentos;
        }


        [HttpGet("{id:int}")]

        public ActionResult<MedicamentosModel> Get(int id)
        {

            var medicamento = _context.Medicamentos.FirstOrDefault(p => p.CodigoId == id);

            if (medicamento is null)
            {
                return NotFound();
            }

            return medicamento;

        }


        [HttpPost]
        public ActionResult Post(MedicamentosModel medicamento)
        {
            _context.Medicamentos.Add(medicamento);
            _context.SaveChanges();

            return new CreatedAtRouteResult("Medicamento Obtido", new { id = medicamento.CodigoId }, medicamento);
        }



        [HttpDelete("{id:int}")]

        public ActionResult Delete(int id)
        {
            var medicamento = _context.Medicamentos.Find(id);
            if (medicamento is null)
            {
                return NotFound("Medicamento nao encontrado na base de dados");
            }

            _context.Medicamentos.Remove(medicamento);
            _context.SaveChanges();
            return Ok(medicamento);


        }
    }
}
