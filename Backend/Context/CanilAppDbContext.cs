using Backend.Models;
using Backend.Models.Insumos;
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
        public DbSet<InsumosModel> Insumos { get; set; }
        public DbSet<ItemNivelEstoqueModel> ItensNivelEstoque { get; set; }
        public DbSet<ItemEstoqueModel> ItensEstoque { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemComEstoqueBaseModel>().ToTable("ItensBase");
            modelBuilder.Entity<ProdutosModel>().ToTable("Produtos");
            modelBuilder.Entity<InsumosModel>().ToTable("Insumos");
            modelBuilder.Entity<MedicamentosModel>().ToTable("Medicamentos");

            modelBuilder.Entity<ItemEstoqueModel>()
                .HasKey(i => new { i.IdItem, i.Lote });

            modelBuilder.Entity<ItemEstoqueModel>()
                .HasOne(i => i.ItemBase)
                .WithMany(p => p.ItensEstoque)
                .HasForeignKey(i => i.IdItem)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemNivelEstoqueModel>()
                .HasKey(i => i.IdItem);

            modelBuilder.Entity<ItemNivelEstoqueModel>()
                .HasOne(i => i.ItemBase)
                .WithOne(p => p.ItemNivelEstoque)
                .HasForeignKey<ItemNivelEstoqueModel>(i => i.IdItem)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ItemComEstoqueBaseModel>()
                .Property(i => i.IdItem)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ProdutosModel>()
                .HasBaseType<ItemComEstoqueBaseModel>();
        }


    }
}
