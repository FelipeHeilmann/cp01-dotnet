using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;

namespace CheckPoint1;

public class CheckpointContext : DbContext
{
    // DbSets para todas as entidades
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoItem> PedidoItens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Configurar conexão com SQLite
        optionsBuilder.UseSqlite("Data Source=loja.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar relacionamento Categoria -> Produtos
        modelBuilder.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Configurar relacionamento Cliente -> Pedidos
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Configurar relacionamento Pedido -> PedidoItens
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(pi => pi.PedidoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Configurar relacionamento Produto -> PedidoItens
        modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Produto)
            .WithMany(p => p.PedidoItens)
            .HasForeignKey(pi => pi.ProdutoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar índices únicos
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();

        // Adicionar dados iniciais
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categorias
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Eletrônicos", Descricao = "Produtos eletrônicos em geral", DataCriacao = DateTime.Now },
            new Categoria { Id = 2, Nome = "Roupas", Descricao = "Vestuário e acessórios", DataCriacao = DateTime.Now },
            new Categoria { Id = 3, Nome = "Casa", Descricao = "Itens para casa e decoração", DataCriacao = DateTime.Now }
        );

        // Seed Produtos
        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, Nome = "Smartphone", Descricao = "Smartphone Android", Preco = 899.99m, Estoque = 50, CategoriaId = 1, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 2, Nome = "Notebook", Descricao = "Notebook Intel Core i5", Preco = 1899.99m, Estoque = 0, CategoriaId = 1, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 3, Nome = "Camiseta", Descricao = "Camiseta 100% algodão", Preco = 29.99m, Estoque = 100, CategoriaId = 2, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 4, Nome = "Calça Jeans", Descricao = "Calça jeans masculina", Preco = 79.99m, Estoque = 30, CategoriaId = 2, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 5, Nome = "Mesa de Centro", Descricao = "Mesa de centro em madeira", Preco = 299.99m, Estoque = 15, CategoriaId = 3, DataCriacao = DateTime.Now, Ativo = true },
            new Produto { Id = 6, Nome = "Luminária", Descricao = "Luminária de mesa LED", Preco = 49.99m, Estoque = 25, CategoriaId = 3, DataCriacao = DateTime.Now, Ativo = true }
        );

        // Seed Clientes
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente 
            { 
                Id = 1, 
                Nome = "João Silva", 
                Email = "joao@email.com", 
                Telefone = "11999999999", 
                CPF = "12345678901", 
                Endereco = "Rua das Flores, 123", 
                Cidade = "São Paulo", 
                Estado = "SP", 
                CEP = "01234567", 
                DataCadastro = DateTime.Now, 
                Ativo = true 
            },
            new Cliente 
            { 
                Id = 2, 
                Nome = "Maria Santos", 
                Email = "maria@email.com", 
                Telefone = "11888888888", 
                CPF = "98765432100", 
                Endereco = "Av. Paulista, 456", 
                Cidade = "São Paulo", 
                Estado = "SP", 
                CEP = "01310100", 
                DataCadastro = DateTime.Now, 
                Ativo = true 
            }
        );

        // Seed Pedidos
        modelBuilder.Entity<Pedido>().HasData(
            new Pedido 
            { 
                Id = 1, 
                NumeroPedido = "PED-001", 
                DataPedido = DateTime.Now.AddDays(-5), 
                Status = StatusPedido.Confirmado, 
                ValorTotal = 929.98m, 
                ClienteId = 1 
            },
            new Pedido 
            { 
                Id = 2, 
                NumeroPedido = "PED-002", 
                DataPedido = DateTime.Now.AddDays(-2), 
                Status = StatusPedido.Pendente, 
                ValorTotal = 379.98m, 
                ClienteId = 2 
            }
        );

        // Seed PedidoItens
        modelBuilder.Entity<PedidoItem>().HasData(
            new PedidoItem { Id = 1, PedidoId = 1, ProdutoId = 1, Quantidade = 1, PrecoUnitario = 899.99m },
            new PedidoItem { Id = 2, PedidoId = 1, ProdutoId = 3, Quantidade = 1, PrecoUnitario = 29.99m },
            new PedidoItem { Id = 3, PedidoId = 2, ProdutoId = 5, Quantidade = 1, PrecoUnitario = 299.99m },
            new PedidoItem { Id = 4, PedidoId = 2, ProdutoId = 4, Quantidade = 1, PrecoUnitario = 79.99m }
        );
    }
}