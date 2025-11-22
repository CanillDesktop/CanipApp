using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLogDeRetirada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetiradaEstoque",
                columns: table => new
                {
                    IdRetirada = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodItem = table.Column<string>(type: "TEXT", nullable: false),
                    NomeItem = table.Column<string>(type: "TEXT", nullable: false),
                    Lote = table.Column<string>(type: "TEXT", nullable: false),
                    De = table.Column<string>(type: "TEXT", nullable: false),
                    Para = table.Column<string>(type: "TEXT", nullable: false),
                    DataHoraInsercaoRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetiradaEstoque", x => x.IdRetirada);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetiradaEstoque");
        }
    }
}
