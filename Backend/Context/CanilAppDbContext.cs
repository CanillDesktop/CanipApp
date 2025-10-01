using Backend.Models.Medicamentos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Context
{
    public class CanilAppDbContext: DbContext
    {
        public CanilAppDbContext(DbContextOptions<CanilAppDbContext> options): base(options)
        {}

        public DbSet<MedicamentosModel> Medicamentos {  get; set; }

    }
}
