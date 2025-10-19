using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class VersaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicamentos",
                columns: table => new
                {
                    CodigoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Prioridade = table.Column<int>(type: "INTEGER", nullable: false),
                    DescricaoMedicamentos = table.Column<string>(type: "TEXT", nullable: false),
                    DataDeEntradaDoMedicamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotaFiscal = table.Column<string>(type: "TEXT", nullable: true),
                    NomeComercial = table.Column<string>(type: "TEXT", nullable: false),
                    PublicoAlvo = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoMensal = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoAnual = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidadeMedicamento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EstoqueDisponivel = table.Column<int>(type: "INTEGER", nullable: false),
                    EntradaEstoque = table.Column<int>(type: "INTEGER", nullable: false),
                    SaidaTotalEstoque = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.CodigoId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicamentos");
        }
    }
}
