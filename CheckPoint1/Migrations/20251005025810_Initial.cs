using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CheckPoint1.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CPF = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    CEP = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estoque = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroPedido = table.Column<string>(type: "TEXT", nullable: false),
                    DataPedido = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Desconto = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PedidoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Desconto = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    PedidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoItens_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "DataCriacao", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9027), "Produtos eletrônicos em geral", "Eletrônicos" },
                    { 2, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9051), "Vestuário e acessórios", "Roupas" },
                    { 3, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9053), "Itens para casa e decoração", "Casa" }
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Ativo", "CEP", "CPF", "Cidade", "DataCadastro", "Email", "Endereco", "Estado", "Nome", "Telefone" },
                values: new object[,]
                {
                    { 1, true, "01234567", "12345678901", "São Paulo", new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9217), "joao@email.com", "Rua das Flores, 123", "SP", "João Silva", "11999999999" },
                    { 2, true, "01310100", "98765432100", "São Paulo", new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9220), "maria@email.com", "Av. Paulista, 456", "SP", "Maria Santos", "11888888888" }
                });

            migrationBuilder.InsertData(
                table: "Pedidos",
                columns: new[] { "Id", "ClienteId", "DataPedido", "Desconto", "NumeroPedido", "Observacoes", "Status", "ValorTotal" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 9, 29, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9239), null, "PED-001", null, 2, 929.98m },
                    { 2, 2, new DateTime(2025, 10, 2, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9246), null, "PED-002", null, 1, 379.98m }
                });

            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Ativo", "CategoriaId", "DataCriacao", "Descricao", "Estoque", "Nome", "Preco" },
                values: new object[,]
                {
                    { 1, true, 1, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9182), "Smartphone Android", 50, "Smartphone", 899.99m },
                    { 2, true, 1, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9185), "Notebook Intel Core i5", 0, "Notebook", 1899.99m },
                    { 3, true, 2, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9187), "Camiseta 100% algodão", 100, "Camiseta", 29.99m },
                    { 4, true, 2, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9190), "Calça jeans masculina", 30, "Calça Jeans", 79.99m },
                    { 5, true, 3, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9192), "Mesa de centro em madeira", 15, "Mesa de Centro", 299.99m },
                    { 6, true, 3, new DateTime(2025, 10, 4, 23, 58, 10, 116, DateTimeKind.Local).AddTicks(9195), "Luminária de mesa LED", 25, "Luminária", 49.99m }
                });

            migrationBuilder.InsertData(
                table: "PedidoItens",
                columns: new[] { "Id", "Desconto", "PedidoId", "PrecoUnitario", "ProdutoId", "Quantidade" },
                values: new object[,]
                {
                    { 1, null, 1, 899.99m, 1, 1 },
                    { 2, null, 1, 29.99m, 3, 1 },
                    { 3, null, 2, 299.99m, 5, 1 },
                    { 4, null, 2, 79.99m, 4, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_PedidoId",
                table: "PedidoItens",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_ProdutoId",
                table: "PedidoItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_NumeroPedido",
                table: "Pedidos",
                column: "NumeroPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaId",
                table: "Produtos",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoItens");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
