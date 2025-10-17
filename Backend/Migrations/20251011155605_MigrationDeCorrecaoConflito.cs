using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MigrationDeCorrecaoConflito : Migration
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

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    IdProduto = table.Column<string>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_Produtos", x => x.IdProduto);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrimeiroNome = table.Column<string>(type: "TEXT", nullable: true),
                    Sobrenome = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    HashSenha = table.Column<string>(type: "TEXT", nullable: true),
                    Permissao = table.Column<int>(type: "INTEGER", nullable: true),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    DataHoraExpiracaoRefreshToken = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataHoraCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "Medicamentos");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
