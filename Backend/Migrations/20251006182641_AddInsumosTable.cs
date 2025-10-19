using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInsumosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    CodigoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DescricaoSimplificada = table.Column<string>(type: "TEXT", nullable: false),
                    DescricaoDetalhada = table.Column<string>(type: "TEXT", nullable: false),
                    DataDeEntradaDoMedicamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotaFiscal = table.Column<string>(type: "TEXT", nullable: true),
                    Unidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoMensal = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoAnual = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidadeInsumo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EstoqueDisponivel = table.Column<int>(type: "INTEGER", nullable: false),
                    EntradaEstoque = table.Column<int>(type: "INTEGER", nullable: false),
                    SaidaTotalEstoque = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.CodigoId);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    CodigoId = table.Column<string>(type: "TEXT", nullable: false),
                    DescricaoSimples = table.Column<string>(type: "TEXT", nullable: true),
                    DataEntrega = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NFe = table.Column<string>(type: "TEXT", nullable: true),
                    DescricaoDetalhada = table.Column<string>(type: "TEXT", nullable: true),
                    Unidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Validade = table.Column<string>(type: "TEXT", nullable: true),
                    DataHoraInsercaoRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstoqueDisponivel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.CodigoId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "Produtos");
        }
    }
}
