using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ReestruturandoInsumosModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Insumos",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "CodigoId",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "ConsumoAnual",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "ConsumoMensal",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "EntradaEstoque",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "EstoqueDisponivel",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "NotaFiscal",
                table: "Insumos");

            migrationBuilder.DropColumn(
                name: "ValidadeInsumo",
                table: "Insumos");

            migrationBuilder.RenameColumn(
                name: "SaidaTotalEstoque",
                table: "Insumos",
                newName: "IdItem");

            migrationBuilder.RenameColumn(
                name: "DataDeEntradaDoMedicamento",
                table: "Insumos",
                newName: "CodInsumo");

            migrationBuilder.AlterColumn<int>(
                name: "IdItem",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Insumos",
                table: "Insumos",
                column: "IdItem");

            migrationBuilder.AddForeignKey(
                name: "FK_Insumos_ItensBase_IdItem",
                table: "Insumos",
                column: "IdItem",
                principalTable: "ItensBase",
                principalColumn: "IdItem",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Insumos_ItensBase_IdItem",
                table: "Insumos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Insumos",
                table: "Insumos");

            migrationBuilder.RenameColumn(
                name: "CodInsumo",
                table: "Insumos",
                newName: "DataDeEntradaDoMedicamento");

            migrationBuilder.RenameColumn(
                name: "IdItem",
                table: "Insumos",
                newName: "SaidaTotalEstoque");

            migrationBuilder.AlterColumn<int>(
                name: "SaidaTotalEstoque",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "CodigoId",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "ConsumoAnual",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsumoMensal",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EntradaEstoque",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstoqueDisponivel",
                table: "Insumos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NotaFiscal",
                table: "Insumos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidadeInsumo",
                table: "Insumos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Insumos",
                table: "Insumos",
                column: "CodigoId");
        }
    }
}
