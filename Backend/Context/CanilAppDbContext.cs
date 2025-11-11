using Backend.Models.Insumos;
using Backend.Models.Medicamentos;
using Backend.Models.Produtos;
using Backend.Models.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // 2. Adicionar o using de Conversores
using System.Globalization;

namespace Backend.Context
{
    public class CanilAppDbContext: DbContext
    {
        public CanilAppDbContext(DbContextOptions<CanilAppDbContext> options): base(options)
        {}
     
        public DbSet<MedicamentosModel> Medicamentos {  get; set; }
        public DbSet<Produtos> Produtos { get; set; }
        public DbSet<UsuariosModel> Usuarios { get; set; }
        public DbSet<InsumosModel> Insumos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            var dateOnlyConverter = new ValueConverter<DateOnly?, string?>(
                v => v.HasValue ? v.Value.ToString("o") : null,
                v => string.IsNullOrEmpty(v) ? null : DateOnly.Parse(v)
            );

            
            modelBuilder.Entity<MedicamentosModel>()
                .Property(m => m.ValidadeMedicamento) 
                .HasConversion(dateOnlyConverter);

            modelBuilder.Entity<InsumosModel>()
                .Property(i => i.ValidadeInsumo) 
                .HasConversion(dateOnlyConverter);

         
        }

    }
}
