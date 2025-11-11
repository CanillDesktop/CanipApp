using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirInsumosModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDeEntradaDoMedicamento",
                table: "Insumos");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDeEntradaDoInsumo",
                table: "Insumos",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDeEntradaDoInsumo",
                table: "Insumos");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataDeEntradaDoMedicamento",
                table: "Insumos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
