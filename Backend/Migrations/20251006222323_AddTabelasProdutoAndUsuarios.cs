using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelasProdutoAndUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
