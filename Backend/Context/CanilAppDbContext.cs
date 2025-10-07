using Backend.Models.Medicamentos;
using Backend.Models.Produtos;
using Backend.Models.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Backend.Context
{
    public class CanilAppDbContext: DbContext
    {
        public CanilAppDbContext(DbContextOptions<CanilAppDbContext> options): base(options)
        {}

        public DbSet<MedicamentosModel> Medicamentos {  get; set; }
        public DbSet<ProdutosModel> Produtos { get; set; }
        public DbSet<UsuariosModel> Usuarios { get; set; }

    }
}
