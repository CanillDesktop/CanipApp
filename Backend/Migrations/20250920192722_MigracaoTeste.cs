using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoTeste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Medicamentos",
                columns: table => new
                {
                    CodigoId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prioridade = table.Column<int>(type: "int", nullable: false),
                    DescricaoMedicamentos = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataEntrega = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NotaFiscal = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeComercial = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorV = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConsumoMensal = table.Column<int>(type: "int", nullable: false),
                    ConsumoAnual = table.Column<int>(type: "int", nullable: false),
                    Validade = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstoqueDisponivel = table.Column<int>(type: "int", nullable: false),
                    EntradaEstoque = table.Column<int>(type: "int", nullable: false),
                    SaidaTotalEstoque = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.CodigoId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicamentos");
        }
    }
}
