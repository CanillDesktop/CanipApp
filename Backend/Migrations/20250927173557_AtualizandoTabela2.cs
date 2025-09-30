using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AtualizandoTabela2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataEntrega",
                table: "Medicamentos");

            migrationBuilder.RenameColumn(
                name: "Validade",
                table: "Medicamentos",
                newName: "DataDeEntradaDoMedicamento");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidadeMedicamento",
                table: "Medicamentos",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidadeMedicamento",
                table: "Medicamentos");

            migrationBuilder.RenameColumn(
                name: "DataDeEntradaDoMedicamento",
                table: "Medicamentos",
                newName: "Validade");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntrega",
                table: "Medicamentos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
