using Backend.Models.Medicamentos;
using Backend.Models.Produtos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Context
{
    public class CanilAppDbContext: DbContext
    {
        public CanilAppDbContext(DbContextOptions<CanilAppDbContext> options): base(options)
        {}

        public DbSet<MedicamentosModel> Medicamentos {  get; set; }
        public DbSet<ProdutosModel> Produtos { get; set; }

        public DbSet<InsumosModel> Insumos { get; set; }

    }
}
