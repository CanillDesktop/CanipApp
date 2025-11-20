using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ReestruturandoMedicamentoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Medicamentos",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "DataHoraInsercaoRegistro",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "CodigoId",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "ConsumoAnual",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "ConsumoMensal",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "EntradaEstoque",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "EstoqueDisponivel",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "NotaFiscal",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "ValidadeMedicamento",
                table: "Medicamentos");

            migrationBuilder.RenameColumn(
                name: "SaidaTotalEstoque",
                table: "Medicamentos",
                newName: "IdItem");

            migrationBuilder.RenameColumn(
                name: "DescricaoMedicamentos",
                table: "Medicamentos",
                newName: "Formula");

            migrationBuilder.RenameColumn(
                name: "DataDeEntradaDoMedicamento",
                table: "Medicamentos",
                newName: "DescricaoMedicamento");

            migrationBuilder.AlterColumn<int>(
                name: "IdItem",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "CodMedicamento",
                table: "Medicamentos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataHoraInsercaoRegistro",
                table: "ItensBase",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Medicamentos",
                table: "Medicamentos",
                column: "IdItem");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicamentos_ItensBase_IdItem",
                table: "Medicamentos",
                column: "IdItem",
                principalTable: "ItensBase",
                principalColumn: "IdItem",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicamentos_ItensBase_IdItem",
                table: "Medicamentos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Medicamentos",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "CodMedicamento",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "DataHoraInsercaoRegistro",
                table: "ItensBase");

            migrationBuilder.RenameColumn(
                name: "Formula",
                table: "Medicamentos",
                newName: "DescricaoMedicamentos");

            migrationBuilder.RenameColumn(
                name: "DescricaoMedicamento",
                table: "Medicamentos",
                newName: "DataDeEntradaDoMedicamento");

            migrationBuilder.RenameColumn(
                name: "IdItem",
                table: "Medicamentos",
                newName: "SaidaTotalEstoque");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataHoraInsercaoRegistro",
                table: "Produtos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "SaidaTotalEstoque",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "CodigoId",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "ConsumoAnual",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsumoMensal",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EntradaEstoque",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstoqueDisponivel",
                table: "Medicamentos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NotaFiscal",
                table: "Medicamentos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidadeMedicamento",
                table: "Medicamentos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Medicamentos",
                table: "Medicamentos",
                column: "CodigoId");
        }
    }
}
