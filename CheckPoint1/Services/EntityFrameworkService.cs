using Microsoft.EntityFrameworkCore;
using CheckPoint1.Models;

namespace CheckPoint1.Services;

public class EntityFrameworkService
{
    private readonly CheckpointContext _context;

    public EntityFrameworkService()
    {
        this._context = new CheckpointContext();
    }

    // ========== CRUD CATEGORIAS ==========
    public void CriarCategoria()
    {
        Console.WriteLine("=== CRIAR CATEGORIA ===");
        Console.Write("Nome da categoria: ");
        var nome = Console.ReadLine();
        Console.Write("Descrição (opcional): ");
        var descricao = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("Nome é obrigatório!");
            return;
        }
        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao,
            DataCriacao = DateTime.Now
        };
        this._context.Categorias.Add(categoria);
        this._context.SaveChanges();
        Console.WriteLine($"Categoria '{nome}' criada com sucesso! Id: {categoria.Id}");
    }
    public void ListarCategorias()
    {
        Console.WriteLine("=== CATEGORIAS ===");
        var categorias = this._context.Categorias
            .Include(c => c.Produtos)
            .OrderBy(c => c.Id)
            .ToList();
        if (!categorias.Any())
        {
            Console.WriteLine("Nenhuma categoria encontrada.");
            return;
        }
        Console.WriteLine();
        foreach (var categoria in categorias)
        {
            Console.WriteLine($"Id: {categoria.Id}");
            Console.WriteLine($"Nome: {categoria.Nome}");
            Console.WriteLine($"Descrição: {categoria.Descricao ?? "Não informada"}");
            Console.WriteLine($"Quantidade de produtos: {categoria.Produtos.Count}");
            Console.WriteLine($"Data de criação: {categoria.DataCriacao:dd/MM/yyyy HH:mm}");
            Console.WriteLine(new string('-', 50));
        }
    }

    // ========== CRUD PRODUTOS ==========
    public void CriarProduto()
    {
        Console.WriteLine("=== CRIAR PRODUTO ===");

        var categorias = this._context.Categorias.OrderBy(c => c.Id).ToList();
        if (!categorias.Any())
        {
            Console.WriteLine("Não existem categorias cadastradas. Cadastre uma categoria primeiro.");
            return;
        }

        Console.WriteLine("Categorias disponíveis:");
        foreach (var cat in categorias)
        {
            Console.WriteLine($"{cat.Id} - {cat.Nome}");
        }

        Console.Write("Digite o Id da categoria: ");
        if (!int.TryParse(Console.ReadLine(), out var categoriaId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var categoria = this._context.Categorias.Find(categoriaId);
        if (categoria == null)
        {
            Console.WriteLine("Categoria não encontrada!");
            return;
        }

        Console.Write("Nome do produto: ");
        var nome = Console.ReadLine();

        Console.Write("Descrição (opcional): ");
        var descricao = Console.ReadLine();

        Console.Write("Preço: R$ ");
        if (!decimal.TryParse(Console.ReadLine(), out var preco) || preco <= 0)
        {
            Console.WriteLine("Preço inválido!");
            return;
        }

        Console.Write("Estoque inicial: ");
        if (!int.TryParse(Console.ReadLine(), out var estoque) || estoque < 0)
        {
            Console.WriteLine("Estoque inválido!");
            return;
        }
        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("Nome é obrigatório!");
            return;
        }

        var produto = new Produto
        {
            Nome = nome,
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao,
            Preco = preco,
            Estoque = estoque,
            CategoriaId = categoriaId,
            DataCriacao = DateTime.Now,
            Ativo = true
        };
        this._context.Produtos.Add(produto);
        this._context.SaveChanges();
        Console.WriteLine($"Produto '{nome}' criado com sucesso! Id: {produto.Id}");
    }
    public void ListarProdutos()
    {
        Console.WriteLine("=== PRODUTOS ===");
        var produtos = this._context.Produtos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Id)
            .ToList();

        if (!produtos.Any())
        {
            Console.WriteLine("Nenhum produto encontrado.");
            return;
        }
        
        Console.WriteLine();
        foreach (var produto in produtos)
        {
            Console.WriteLine($"Id: {produto.Id}");
            Console.WriteLine($"Nome: {produto.Nome}");
            Console.WriteLine($"Descrição: {produto.Descricao ?? "Não informada"}");
            Console.WriteLine($"Categoria: {produto.Categoria.Nome}");
            Console.WriteLine($"Preço: R$ {produto.Preco:F2}");
            Console.WriteLine($"Estoque: {produto.Estoque}");
            Console.WriteLine($"Status: {(produto.Ativo ? "Ativo" : "Inativo")}");
            Console.WriteLine($"Data de criação: {produto.DataCriacao:dd/MM/yyyy HH:mm}");
            Console.WriteLine(new string('-', 50));
        }
    }
    public void AtualizarProduto()
    {
        Console.WriteLine("=== ATUALIZAR PRODUTO ===");

        Console.Write("Digite o Id do produto a ser atualizado: ");
        if (!int.TryParse(Console.ReadLine(), out var produtoId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var produto = this._context.Produtos.Include(p => p.Categoria).FirstOrDefault(p => p.Id == produtoId);
        if (produto == null)
        {
            Console.WriteLine("Produto não encontrado!");
            return;
        }

        Console.WriteLine($"Produto atual: {produto.Nome} - R$ {produto.Preco:F2} - Estoque: {produto.Estoque}");
        Console.WriteLine($"Categoria: {produto.Categoria.Nome}");
        Console.WriteLine();

        Console.Write($"Novo nome [{produto.Nome}]: ");
        var novoNome = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoNome))
            produto.Nome = novoNome;

        Console.Write($"Nova descrição [{produto.Descricao ?? "Vazio"}]: ");
        var novaDescricao = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novaDescricao))
            produto.Descricao = novaDescricao;

        Console.Write($"Novo preço [{produto.Preco:F2}]: R$ ");
        var precoInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(precoInput) && decimal.TryParse(precoInput, out var novoPreco) && novoPreco > 0)
            produto.Preco = novoPreco;

        Console.Write($"Novo estoque [{produto.Estoque}]: ");
        var estoqueInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(estoqueInput) && int.TryParse(estoqueInput, out var novoEstoque) && novoEstoque >= 0)
            produto.Estoque = novoEstoque;

        Console.Write($"Ativo (S/N) [{(produto.Ativo ? "S" : "N")}]: ");
        var ativoInput = Console.ReadLine()?.ToUpper();
        if (ativoInput == "S" || ativoInput == "N")
            produto.Ativo = ativoInput == "S";

        this._context.SaveChanges();
        Console.WriteLine("Produto atualizado com sucesso!");
    }

    // ========== CRUD CLIENTES ==========
    public void CriarCliente()
    {
        Console.WriteLine("=== CRIAR CLIENTE ===");

        Console.Write("Nome completo: ");
        var nome = Console.ReadLine();

        Console.Write("Email: ");
        var email = Console.ReadLine();

        Console.Write("Telefone (opcional): ");
        var telefone = Console.ReadLine();

        Console.Write("CPF (999.999.999-99): ");
        var cpfInput = Console.ReadLine();

        Console.Write("Endereço (opcional): ");
        var endereco = Console.ReadLine();

        Console.Write("Cidade (opcional): ");
        var cidade = Console.ReadLine();

        Console.Write("Estado (opcional): ");
        var estado = Console.ReadLine();

        Console.Write("CEP (99999-999): ");
        var cep = new string(Console.ReadLine()?.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(nome))
        {
            Console.WriteLine("Nome é obrigatório!");
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email é obrigatório!");
            return;
        }

        if (this._context.Clientes.Any(c => c.Email == email))
        {
            Console.WriteLine("Este email já está cadastrado!");
            return;
        }

        string? cpf = null;
        if (!string.IsNullOrWhiteSpace(cpfInput))
        {
            cpf = new string(cpfInput.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11)
            {
                Console.WriteLine("CPF deve ter 11 dígitos!");
                return;
            }
        }

        var cliente = new Cliente
        {
            Nome = nome,
            Email = email,
            Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone,
            CPF = cpf,
            Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco,
            Cidade = string.IsNullOrWhiteSpace(cidade) ? null : cidade,
            Estado = string.IsNullOrWhiteSpace(estado) ? null : estado,
            CEP = string.IsNullOrWhiteSpace(cep) ? null : cep,
            DataCadastro = DateTime.Now,
            Ativo = true
        };

        this._context.Clientes.Add(cliente);
        this._context.SaveChanges();

        Console.WriteLine($"Cliente '{nome}' criado com sucesso! Id: {cliente.Id}");

    }
    public void ListarClientes()
    {
        Console.WriteLine("=== CLIENTES ===");

        var clientes = this._context.Clientes
            .Include(c => c.Pedidos)
            .OrderBy(c => c.Id)
            .ToList();

        if (!clientes.Any())
        {
            Console.WriteLine("Nenhum cliente encontrado.");
            return;
        }

        Console.WriteLine();
        foreach (var cliente in clientes)
        {
            Console.WriteLine($"Id: {cliente.Id}");
            Console.WriteLine($"Nome: {cliente.Nome}");
            Console.WriteLine($"Email: {cliente.Email}");
            Console.WriteLine($"Telefone: {cliente.Telefone ?? "Não informado"}");
            Console.WriteLine($"CPF: {cliente.CPF ?? "Não informado"}");
            Console.WriteLine($"Endereço: {cliente.Endereco ?? "Não informado"}");
            Console.WriteLine($"Cidade/Estado: {cliente.Cidade ?? "N/A"}/{cliente.Estado ?? "N/A"}");
            Console.WriteLine($"CEP: {cliente.CEP ?? "Não informado"}");
            Console.WriteLine($"Quantidade de pedidos: {cliente.Pedidos.Count}");
            Console.WriteLine($"Status: {(cliente.Ativo ? "Ativo" : "Inativo")}");
            Console.WriteLine($"Data de cadastro: {cliente.DataCadastro:dd/MM/yyyy HH:mm}");
            Console.WriteLine(new string('-', 50));
        }
    }
    public void AtualizarCliente()
    {
        Console.WriteLine("=== ATUALIZAR CLIENTE ===");

        Console.Write("Digite o Id do cliente a ser atualizado: ");
        if (!int.TryParse(Console.ReadLine(), out var clienteId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var cliente = this._context.Clientes.Find(clienteId);
        if (cliente == null)
        {
            Console.WriteLine("Cliente não encontrado!");
            return;
        }

        Console.WriteLine($"Cliente atual: {cliente.Nome} - {cliente.Email}");
        Console.WriteLine();

        Console.Write($"Novo nome [{cliente.Nome}]: ");
        var novoNome = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoNome))
            cliente.Nome = novoNome;

        Console.Write($"Novo email [{cliente.Email}]: ");
        var novoEmail = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoEmail))
        {
            if (this._context.Clientes.Any(c => c.Email == novoEmail && c.Id != clienteId))
            {
                Console.WriteLine("Este email já está sendo usado por outro cliente!");
                return;
            }
            cliente.Email = novoEmail;
        }

        Console.Write($"Novo telefone [{cliente.Telefone ?? "Vazio"}]: ");
        var novoTelefone = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoTelefone))
            cliente.Telefone = novoTelefone;

        Console.Write($"Novo endereço [{cliente.Endereco ?? "Vazio"}]: ");
        var novoEndereco = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoEndereco))
            cliente.Endereco = novoEndereco;

        Console.Write($"Nova cidade [{cliente.Cidade ?? "Vazio"}]: ");
        var novaCidade = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novaCidade))
            cliente.Cidade = novaCidade;

        Console.Write($"Novo estado [{cliente.Estado ?? "Vazio"}]: ");
        var novoEstado = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoEstado))
            cliente.Estado = novoEstado;

        Console.Write($"Novo CEP [{cliente.CEP ?? "Vazio"}]: ");
        var novoCep = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(novoCep))
            cliente.CEP = novoCep;

        Console.Write($"Ativo (S/N) [{(cliente.Ativo ? "S" : "N")}]: ");
        var ativoInput = Console.ReadLine()?.ToUpper();
        if (ativoInput == "S" || ativoInput == "N")
            cliente.Ativo = ativoInput == "S";

        this._context.SaveChanges();
        Console.WriteLine("Cliente atualizado com sucesso!");
    }

    // ========== CRUD PEDIDOS ==========
    public void CriarPedido()
    {
        Console.WriteLine("=== CRIAR PEDIDO ===");
        var clientes = this._context.Clientes.Where(c => c.Ativo).OrderBy(c => c.Id).ToList();
        if (!clientes.Any())
        {
            Console.WriteLine("Não existem clientes ativos cadastrados.");
            return;
        }

        Console.WriteLine("Clientes disponíveis:");
        foreach (var cli in clientes)
        {
            Console.WriteLine($"{cli.Id} - {cli.Nome} ({cli.Email})");
        }

        Console.Write("Digite o Id do cliente: ");
        if (!int.TryParse(Console.ReadLine(), out var clienteId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var cliente = this._context.Clientes.Find(clienteId);
        if (cliente == null || !cliente.Ativo)
        {
            Console.WriteLine("Cliente não encontrado ou inativo!");
            return;
        }

        var ultimoPedido = this._context.Pedidos.OrderByDescending(p => p.Id).FirstOrDefault();
        var proximoNumero = ultimoPedido?.Id + 1 ?? 1;
        var numeroPedido = $"PED-{proximoNumero:D3}";

        var pedido = new Pedido
        {
            NumeroPedido = numeroPedido,
            DataPedido = DateTime.Now,
            Status = StatusPedido.Pendente,
            ClienteId = clienteId,
            ValorTotal = 0
        };

        this._context.Pedidos.Add(pedido);
        this._context.SaveChanges();

        Console.WriteLine($"Pedido {numeroPedido} criado! Agora adicione os itens.");
        Console.WriteLine();

        var continuarAdicionando = true;
        var valorTotal = 0.0;

        while (continuarAdicionando)
        {
            var produtos = this._context.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Ativo && p.Estoque > 0)
                .OrderBy(p => p.Id)
                .ToList();

            if (!produtos.Any())
            {
                Console.WriteLine("Não há produtos disponíveis em estoque.");
                break;
            }

            Console.WriteLine("Produtos disponíveis:");
            foreach (var prod in produtos)
            {
                Console.WriteLine($"{prod.Id} - {prod.Nome} - R$ {prod.Preco:F2} (Estoque: {prod.Estoque})");
            }

            Console.Write("Digite o Id do produto (0 para finalizar): ");
            if (!int.TryParse(Console.ReadLine(), out var produtoId) || produtoId == 0)
            {
                break;
            }

            var produto = produtos.FirstOrDefault(p => p.Id == produtoId);
            if (produto == null)
            {
                Console.WriteLine("Produto não encontrado ou indisponível!");
                continue;
            }

            Console.Write($"Quantidade (máx: {produto.Estoque}): ");
            if (!int.TryParse(Console.ReadLine(), out var quantidade) || quantidade <= 0)
            {
                Console.WriteLine("Quantidade inválida!");
                continue;
            }

            if (quantidade > produto.Estoque)
            {
                Console.WriteLine($"Estoque insuficiente! Disponível: {produto.Estoque}");
                continue;
            }

            var item = new PedidoItem
            {
                PedidoId = pedido.Id,
                ProdutoId = produtoId,
                Quantidade = quantidade,
                PrecoUnitario = produto.Preco
            };

            this._context.PedidoItens.Add(item);

            produto.Estoque -= quantidade;

            valorTotal += (double)item.Subtotal;

            Console.WriteLine($"Item adicionado: {produto.Nome} x{quantidade} = R$ {item.Subtotal:F2}");

            Console.Write("Adicionar outro item? (S/N): ");
            continuarAdicionando = Console.ReadLine()?.ToUpper() == "S";
        }

        pedido.ValorTotal = (decimal)valorTotal;
        this._context.SaveChanges();

        Console.WriteLine($"Pedido {numeroPedido} finalizado com sucesso!");
        Console.WriteLine($"Valor total: R$ {valorTotal:F2}");
    }
    public void ListarPedidos()
    {
        Console.WriteLine("=== PEDIDOS ===");

        var pedidos = this._context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .OrderByDescending(p => p.DataPedido)
            .ToList();

        if (!pedidos.Any())
        {
            Console.WriteLine("Nenhum pedido encontrado.");
            return;
        }

        Console.WriteLine();
        foreach (var pedido in pedidos)
        {
            Console.WriteLine($"Id: {pedido.Id}");
            Console.WriteLine($"Número: {pedido.NumeroPedido}");
            Console.WriteLine($"Cliente: {pedido.Cliente.Nome} ({pedido.Cliente.Email})");
            Console.WriteLine($"Data: {pedido.DataPedido:dd/MM/yyyy HH:mm}");
            Console.WriteLine($"Status: {pedido.Status}");
            Console.WriteLine($"Valor Total: R$ {pedido.ValorTotal:F2}");

            if (pedido.Desconto.HasValue && pedido.Desconto > 0)
                Console.WriteLine($"Desconto: R$ {pedido.Desconto:F2}");

            if (!string.IsNullOrWhiteSpace(pedido.Observacoes))
                Console.WriteLine($"Observações: {pedido.Observacoes}");

            Console.WriteLine("Itens:");
            foreach (var item in pedido.Itens)
            {
                Console.WriteLine($"  - {item.Produto.Nome} x{item.Quantidade} = R$ {item.Subtotal:F2}");
                Console.WriteLine($"    Preço unitário: R$ {item.PrecoUnitario:F2}");
                if (item.Desconto.HasValue && item.Desconto > 0)
                    Console.WriteLine($"    Desconto: R$ {item.Desconto:F2}");
            }

            Console.WriteLine(new string('-', 50));
        }
    }
    public void AtualizarStatusPedido()
    {
        Console.WriteLine("=== ATUALIZAR STATUS PEDIDO ===");
        this.ListarPedidos();
        Console.Write("Digite o Id do pedido: ");
        if (!int.TryParse(Console.ReadLine(), out var pedidoId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var pedido = this._context.Pedidos
            .Include(p => p.Cliente)
            .FirstOrDefault(p => p.Id == pedidoId);

        if (pedido == null)
        {
            Console.WriteLine("Pedido não encontrado!");
            return;
        }

        Console.WriteLine($"Pedido: {pedido.NumeroPedido}");
        Console.WriteLine($"Cliente: {pedido.Cliente.Nome}");
        Console.WriteLine($"Status atual: {pedido.Status}");
        Console.WriteLine();

        Console.WriteLine("Status disponíveis:");
        var statusValidos = Enum.GetValues<StatusPedido>().ToList();
        foreach (var status in statusValidos)
        {
            Console.WriteLine($"{(int)status} - {status}");
        }

        Console.Write("Digite o novo status: ");
        if (!int.TryParse(Console.ReadLine(), out var novoStatusInt) ||
            !Enum.IsDefined(typeof(StatusPedido), novoStatusInt))
        {
            Console.WriteLine("Status inválido!");
            return;
        }

        var novoStatus = (StatusPedido)novoStatusInt;

        if (!ValidarTransicaoStatus(pedido.Status, novoStatus))
        {
            Console.WriteLine($"Transição inválida de {pedido.Status} para {novoStatus}!");
            return;
        }

        pedido.Status = novoStatus;
        this._context.SaveChanges();

        Console.WriteLine($"Status do pedido {pedido.NumeroPedido} atualizado para {novoStatus}!");

    }
    private bool ValidarTransicaoStatus(StatusPedido statusAtual, StatusPedido novoStatus)
    {
        if (statusAtual == StatusPedido.Cancelado)
            return false;

        if (statusAtual == StatusPedido.Entregue && novoStatus != StatusPedido.Cancelado)
            return false;

        if (novoStatus != StatusPedido.Cancelado && (int)novoStatus < (int)statusAtual)
            return false;

        return true;
    }
    public void CancelarPedido()
    {
        Console.WriteLine("=== CANCELAR PEDIDO ===");
        this.ListarPedidos();
        Console.Write("Digite o Id do pedido: ");
        if (!int.TryParse(Console.ReadLine(), out var pedidoId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var pedido = this._context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefault(p => p.Id == pedidoId);

        if (pedido == null)
        {
            Console.WriteLine("Pedido não encontrado!");
            return;
        }

        Console.WriteLine($"Pedido: {pedido.NumeroPedido}");
        Console.WriteLine($"Cliente: {pedido.Cliente.Nome}");
        Console.WriteLine($"Status atual: {pedido.Status}");
        Console.WriteLine($"Valor: R$ {pedido.ValorTotal:F2}");

        if (pedido.Status != StatusPedido.Pendente && pedido.Status != StatusPedido.Confirmado)
        {
            Console.WriteLine($"Não é possível cancelar pedido com status '{pedido.Status}'!");
            Console.WriteLine("Apenas pedidos Pendentes ou Confirmados podem ser cancelados.");
            return;
        }

        Console.Write("Confirma o cancelamento? (S/N): ");
        if (Console.ReadLine()?.ToUpper() != "S")
        {
            Console.WriteLine("Cancelamento abortado.");
            return;
        }

        foreach (var item in pedido.Itens)
        {
            item.Produto.Estoque += item.Quantidade;
            Console.WriteLine($"Estoque devolvido: {item.Produto.Nome} +{item.Quantidade}");
        }

        pedido.Status = StatusPedido.Cancelado;
        this._context.SaveChanges();

        Console.WriteLine($"Pedido {pedido.NumeroPedido} cancelado com sucesso!");
        Console.WriteLine("Estoque dos produtos foi restaurado.");
    }

    // ========== CONSULTAS LINQ AVANÇADAS ==========
    public void ConsultasAvancadas()
    {
        Console.WriteLine("=== CONSULTAS LINQ ===");
        Console.WriteLine("1. Produtos mais vendidos");
        Console.WriteLine("2. Clientes com mais pedidos");
        Console.WriteLine("3. Faturamento por categoria");
        Console.WriteLine("4. Pedidos por período");
        Console.WriteLine("5. Produtos em estoque baixo");


        var opcao = Console.ReadLine();

        switch (opcao)
        {
            case "1": ProdutosMaisVendidos(); break;
            case "2": ClientesComMaisPedidos(); break;
            case "3": FaturamentoPorCategoria(); break;
            case "4": PedidosPorPeriodo(); break;
            case "5": ProdutosEstoqueBaixo(); break;
            case "6": AnaliseVendasMensal(); break;
            case "7": TopClientesPorValor(); break;
        }
    }
    private void ProdutosMaisVendidos()
    {
        Console.WriteLine("=== PRODUTOS MAIS VENDIDOS ===");

        var produtosMaisVendidos = this._context.PedidoItens
            .Include(pi => pi.Produto)
                .ThenInclude(p => p.Categoria)
            .GroupBy(pi => new { pi.ProdutoId, pi.Produto.Nome, CategoriaNome = pi.Produto.Categoria.Nome })
            .Select(g => new
            {
                ProdutoId = g.Key.ProdutoId,
                NomeProduto = g.Key.Nome,
                NomeCategoria = g.Key.CategoriaNome,
                QuantidadeVendida = g.Sum(pi => pi.Quantidade),
                ValorTotal = g.Sum(pi => (double)(pi.Quantidade * pi.PrecoUnitario - (pi.Desconto ?? 0)))
            })
            .OrderByDescending(x => x.QuantidadeVendida)
            .ToList();

        if (!produtosMaisVendidos.Any())
        {
            Console.WriteLine("Nenhuma venda encontrada.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Produto",-30} {"Categoria",-15} {"Qtd Vendida",-12} {"Valor Total",-12}");
        Console.WriteLine(new string('-', 70));

        foreach (var item in produtosMaisVendidos)
        {
            Console.WriteLine($"{item.NomeProduto,-30} {item.NomeCategoria,-15} {item.QuantidadeVendida,-12} R$ {item.ValorTotal,-9:F2}");
        }
    }
    private void ClientesComMaisPedidos()
    {
        Console.WriteLine("=== CLIENTES COM MAIS PEDIDOS ===");

        var clientesComMaisPedidos = this._context.Clientes
            .Include(c => c.Pedidos)
            .Select(c => new
            {
                ClienteId = c.Id,
                Nome = c.Nome,
                Email = c.Email,
                QuantidadePedidos = c.Pedidos.Count,
                ValorTotal = c.Pedidos.Sum(p => (double)p.ValorTotal)
            })
            .Where(x => x.QuantidadePedidos > 0)
            .OrderByDescending(x => x.QuantidadePedidos)
            .ThenByDescending(x => x.ValorTotal)
            .ToList();

        if (!clientesComMaisPedidos.Any())
        {
            Console.WriteLine("Nenhum cliente com pedidos encontrado.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Cliente",-30} {"Email",-25} {"Qtd Pedidos",-12} {"Valor Total",-12}");
        Console.WriteLine(new string('-', 80));

        foreach (var cliente in clientesComMaisPedidos)
        {
            Console.WriteLine($"{cliente.Nome,-30} {cliente.Email,-25} {cliente.QuantidadePedidos,-12} R$ {cliente.ValorTotal,-9:F2}");
        }

    }
    private void FaturamentoPorCategoria()
    {
        Console.WriteLine("=== FATURAMENTO POR CATEGORIA ===");

        var faturamentoPorCategoria = this._context.PedidoItens
            .Include(pi => pi.Produto)
                .ThenInclude(p => p.Categoria)
            .GroupBy(pi => pi.Produto.Categoria.Nome)
            .Select(g => new
            {
                Categoria = g.Key,
                ValorTotalVendido = g.Sum(pi => (double)(pi.Quantidade * pi.PrecoUnitario - (pi.Desconto ?? 0))),
                QuantidadeProdutosVendidos = g.Sum(pi => pi.Quantidade),
                TicketMedio = g.Average(pi => (double)(pi.Quantidade * pi.PrecoUnitario - (pi.Desconto ?? 0)))
            })
            .OrderByDescending(x => x.ValorTotalVendido)
            .ToList();

        if (!faturamentoPorCategoria.Any())
        {
            Console.WriteLine("Nenhuma venda encontrada.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Categoria",-20} {"Faturamento",-15} {"Qtd Vendida",-12} {"Ticket Médio",-12}");
        Console.WriteLine(new string('-', 65));

        foreach (var item in faturamentoPorCategoria)
        {
            Console.WriteLine($"{item.Categoria,-20} R$ {item.ValorTotalVendido,-12:F2} {item.QuantidadeProdutosVendidos,-12} R$ {item.TicketMedio,-9:F2}");
        }

        var totalGeral = faturamentoPorCategoria.Sum(x => x.ValorTotalVendido);
        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"{"TOTAL GERAL",-20} R$ {totalGeral,-12:F2}");
    }
    private void PedidosPorPeriodo()
    {
        Console.WriteLine("=== PEDIDOS POR PERÍODO ===");

        Console.Write("Data início (dd/mm/aaaa): ");
        if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataInicio))
        {
            Console.WriteLine("Data inválida!");
            return;
        }

        Console.Write("Data fim (dd/mm/aaaa): ");
        if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataFim))
        {
            Console.WriteLine("Data inválida!");
            return;
        }

        var diaAposDataFim = dataFim.AddDays(1);

        var pedidosPorPeriodo = this._context.Pedidos
            .Where(p => p.DataPedido >= dataInicio && p.DataPedido < diaAposDataFim)
            .GroupBy(p => p.DataPedido.Date)
            .Select(g => new
            {
                Data = g.Key,
                QuantidadePedidos = g.Count(),
                ValorTotal = g.Sum(p => (double)p.ValorTotal),
                TicketMedio = g.Average(p => (double)p.ValorTotal)
            })
            .OrderBy(x => x.Data)
            .ToList();

        if (!pedidosPorPeriodo.Any())
        {
            Console.WriteLine("Nenhum pedido encontrado no período.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Data",-12} {"Qtd Pedidos",-12} {"Valor Total",-15} {"Ticket Médio",-12}");
        Console.WriteLine(new string('-', 55));

        foreach (var item in pedidosPorPeriodo)
        {
            Console.WriteLine($"{item.Data:dd/MM/yyyy}  {item.QuantidadePedidos,-12} R$ {item.ValorTotal,-12:F2} R$ {item.TicketMedio,-9:F2}");
        }

        var totalPedidos = pedidosPorPeriodo.Sum(x => x.QuantidadePedidos);
        var totalValor = pedidosPorPeriodo.Sum(x => x.ValorTotal);
        Console.WriteLine(new string('-', 55));
        Console.WriteLine($"TOTAIS:      {totalPedidos,-12} R$ {totalValor,-12:F2}");
    }
    private void ProdutosEstoqueBaixo()
    {
        Console.WriteLine("=== PRODUTOS EM ESTOQUE BAIXO ===");

        var produtosEstoqueBaixo = this._context.Produtos
            .Include(p => p.Categoria)
            .Where(p => p.Ativo && p.Estoque < 20)
            .OrderBy(p => p.Estoque)
            .ThenBy(p => p.Nome)
            .ToList();

        if (!produtosEstoqueBaixo.Any())
        {
            Console.WriteLine("Nenhum produto com estoque baixo.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Produto",-30} {"Categoria",-15} {"Estoque",-8} {"Preço",-10}");
        Console.WriteLine(new string('-', 65));

        foreach (var produto in produtosEstoqueBaixo)
        {
            var alerta = produto.Estoque == 0 ? "[ZERADO]" : produto.Estoque < 10 ? "[CRÍTICO]" : "[BAIXO]";
            Console.WriteLine($"{produto.Nome,-30} {produto.Categoria.Nome,-15} {produto.Estoque,-8} R$ {produto.Preco,-7:F2} {alerta}");
        }

        var totalProdutos = produtosEstoqueBaixo.Count;
        var produtosZerados = produtosEstoqueBaixo.Count(p => p.Estoque == 0);
        Console.WriteLine();
        Console.WriteLine($"Total com estoque baixo: {totalProdutos}");
        Console.WriteLine($"Produtos zerados: {produtosZerados}");
    }
    private void AnaliseVendasMensal()
    {
        Console.WriteLine("=== ANÁLISE DE VENDAS MENSAL ===");

        var vendasMensais = this._context.Pedidos
            .Where(p => p.Status != StatusPedido.Cancelado)
            .GroupBy(p => new { Ano = p.DataPedido.Year, Mes = p.DataPedido.Month })
            .Select(g => new
            {
                Ano = g.Key.Ano,
                Mes = g.Key.Mes,
                QuantidadeVendida = g.Sum(p => p.Itens.Sum(i => i.Quantidade)),
                Faturamento = g.Sum(p => p.ValorTotal),
                QuantidadePedidos = g.Count()
            })
            .OrderBy(x => x.Ano)
            .ThenBy(x => x.Mes)
            .ToList();

        if (!vendasMensais.Any())
        {
            Console.WriteLine("Nenhuma venda encontrada.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Período",-12} {"Pedidos",-8} {"Qtd Itens",-10} {"Faturamento",-15} {"Crescimento",-12}");
        Console.WriteLine(new string('-', 65));

        decimal? faturamentoAnterior = null;

        foreach (var venda in vendasMensais)
        {
            var periodo = $"{venda.Mes:D2}/{venda.Ano}";
            var crescimento = "N/A";

            if (faturamentoAnterior.HasValue && faturamentoAnterior > 0)
            {
                var percentual = (venda.Faturamento - faturamentoAnterior.Value) / faturamentoAnterior.Value * 100;
                crescimento = $"{percentual:+0.0;-0.0;0.0}%";
            }

            Console.WriteLine($"{periodo,-12} {venda.QuantidadePedidos,-8} {venda.QuantidadeVendida,-10} R$ {venda.Faturamento,-12:F2} {crescimento,-12}");
            faturamentoAnterior = venda.Faturamento;
        }
    }
    private void TopClientesPorValor()
    {
        Console.WriteLine("=== TOP 10 CLIENTES POR VALOR ===");


        var topClientes = this._context.Clientes
            .Select(c => new
            {
                Cliente = c,
                ValorTotalPedidos = c.Pedidos
                    .Where(p => p.Status != StatusPedido.Cancelado)
                    .Sum(p => p.ValorTotal),
                QuantidadePedidos = c.Pedidos
                    .Where(p => p.Status != StatusPedido.Cancelado)
                    .Count()
            })
            .Where(x => x.ValorTotalPedidos > 0)
            .OrderByDescending(x => x.ValorTotalPedidos)
            .Take(10)
            .ToList();

        if (!topClientes.Any())
        {
            Console.WriteLine("Nenhum cliente com compras encontrado.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"#",-3} {"Cliente",-30} {"Email",-25} {"Pedidos",-8} {"Valor Total",-12}");
        Console.WriteLine(new string('-', 80));

        for (var i = 0; i < topClientes.Count; i++)
        {
            var cliente = topClientes[i];
            Console.WriteLine($"{i + 1,-3} {cliente.Cliente.Nome,-30} {cliente.Cliente.Email,-25} {cliente.QuantidadePedidos,-8} R$ {cliente.ValorTotalPedidos,-9:F2}");
        }

        var totalFaturamento = topClientes.Sum(x => x.ValorTotalPedidos);
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total dos Top 10: R$ {totalFaturamento:F2}");
    }

    // ========== RELATÓRIOS GERAIS ==========
    public void RelatoriosGerais()
    {
        Console.WriteLine("=== RELATÓRIOS GERAIS ===");
        Console.WriteLine("1. Dashboard executivo");
        Console.WriteLine("2. Relatório de estoque");
        Console.WriteLine("3. Análise de clientes");

        var opcao = Console.ReadLine();

        switch (opcao)
        {
            case "1": DashboardExecutivo(); break;
            case "2": RelatorioEstoque(); break;
            case "3": AnaliseClientes(); break;
        }
    }
    private void DashboardExecutivo()
    {
        Console.WriteLine("=== DASHBOARD EXECUTIVO ===");

        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1);

        var totalPedidos = this._context.Pedidos.Count();
        var pedidosMes = this._context.Pedidos.Count(p => p.DataPedido >= inicioMes && p.DataPedido < fimMes);

        var ticketMedio = this._context.Pedidos
            .Where(p => p.Status != StatusPedido.Cancelado)
            .Average(p => (double?)p.ValorTotal) ?? 0;

        var produtosAtivos = this._context.Produtos.Count(p => p.Ativo);
        var produtosEstoque = this._context.Produtos.Where(p => p.Ativo).Sum(p => p.Estoque);
        var produtosZerados = this._context.Produtos.Count(p => p.Ativo && p.Estoque == 0);

        var clientesAtivos = this._context.Clientes.Count(c => c.Ativo);
        var clientesComPedidos = this._context.Clientes.Count(c => c.Pedidos.Any());

        var faturamentoMes = this._context.Pedidos
            .Where(p => p.DataPedido >= inicioMes && p.DataPedido < fimMes && p.Status != StatusPedido.Cancelado)
            .Sum(p => (double?)p.ValorTotal) ?? 0;

        var faturamentoTotal = this._context.Pedidos
            .Where(p => p.Status != StatusPedido.Cancelado)
            .Sum(p => (double?)p.ValorTotal) ?? 0;

        Console.WriteLine();
        Console.WriteLine("===== RESUMO GERAL =====");
        Console.WriteLine($"Total de pedidos: {totalPedidos}");
        Console.WriteLine($"Pedidos este mês: {pedidosMes}");
        Console.WriteLine($"Ticket médio: R$ {ticketMedio:F2}");
        Console.WriteLine();

        Console.WriteLine("===== ESTOQUE =====");
        Console.WriteLine($"Produtos ativos: {produtosAtivos}");
        Console.WriteLine($"Itens em estoque: {produtosEstoque}");
        Console.WriteLine($"Produtos zerados: {produtosZerados}");
        Console.WriteLine();

        Console.WriteLine("===== CLIENTES =====");
        Console.WriteLine($"Clientes ativos: {clientesAtivos}");
        Console.WriteLine($"Clientes com pedidos: {clientesComPedidos}");
        Console.WriteLine();

        Console.WriteLine("===== FATURAMENTO =====");
        Console.WriteLine($"Faturamento total: R$ {faturamentoTotal:F2}");
        Console.WriteLine($"Faturamento este mês: R$ {faturamentoMes:F2}");

        var percentualMes = faturamentoTotal > 0 ? (faturamentoMes / faturamentoTotal) * 100 : 0;
        Console.WriteLine($"% do faturamento no mês: {percentualMes:F1}%");
    }
    private void RelatorioEstoque()
    {
        Console.WriteLine("=== RELATÓRIO DE ESTOQUE ===");
        var estoqueCategoria = this._context.Produtos
            .Include(p => p.Categoria)
            .Where(p => p.Ativo)
            .GroupBy(p => p.Categoria.Nome)
            .Select(g => new
            {
                Categoria = g.Key,
                QuantidadeProdutos = g.Count(),
                EstoqueTotal = g.Sum(p => p.Estoque),
                ValorEstoque = g.Sum(p => p.Estoque * (double)p.Preco),
                ProdutosZerados = g.Count(p => p.Estoque == 0)
            })
            .OrderBy(x => x.Categoria)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("===== ESTOQUE POR CATEGORIA =====");
        Console.WriteLine($"{"Categoria",-20} {"Produtos",-10} {"Estoque",-10} {"Valor",-15} {"Zerados",-8}");
        Console.WriteLine(new string('-', 65));

        var valorTotalEstoque = 0.0;
        var totalProdutos = 0;
        var totalEstoque = 0;
        var totalZerados = 0;

        foreach (var item in estoqueCategoria)
        {
            Console.WriteLine($"{item.Categoria,-20} {item.QuantidadeProdutos,-10} {item.EstoqueTotal,-10} R$ {item.ValorEstoque,-12:F2} {item.ProdutosZerados,-8}");
            valorTotalEstoque += (double)item.ValorEstoque;
            totalProdutos += item.QuantidadeProdutos;
            totalEstoque += item.EstoqueTotal;
            totalZerados += item.ProdutosZerados;
        }

        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"{"TOTAIS",-20} {totalProdutos,-10} {totalEstoque,-10} R$ {valorTotalEstoque,-12:F2} {totalZerados,-8}");

        var produtosBaixoEstoque = this._context.Produtos
            .Include(p => p.Categoria)
            .Where(p => p.Ativo && p.Estoque < 20)
            .OrderBy(p => p.Estoque)
            .ToList();

        if (produtosBaixoEstoque.Any())
        {
            Console.WriteLine();
            Console.WriteLine("===== PRODUTOS EM ESTOQUE BAIXO =====");
            Console.WriteLine($"{"Produto",-30} {"Categoria",-15} {"Estoque",-8}");
            Console.WriteLine(new string('-', 55));

            foreach (var produto in produtosBaixoEstoque)
            {
                Console.WriteLine($"{produto.Nome,-30} {produto.Categoria.Nome,-15} {produto.Estoque,-8}");
            }
        }
    }
    private void AnaliseClientes()
    {
        Console.WriteLine("=== ANÁLISE DE CLIENTES ===");

        var clientesPorEstado = this._context.Clientes
            .Where(c => c.Ativo && !string.IsNullOrEmpty(c.Estado))
            .GroupBy(c => c.Estado)
            .Select(g => new
            {
                Estado = g.Key,
                QuantidadeClientes = g.Count(),
                ClientesComPedidos = g.Count(c => c.Pedidos.Any()),
                ValorTotalPedidos = g.Sum(c => c.Pedidos.Where(p => p.Status != StatusPedido.Cancelado).Sum(p => (double)p.ValorTotal))
            })
            .OrderByDescending(x => x.QuantidadeClientes)
            .ToList();

        Console.WriteLine();
        Console.WriteLine("===== CLIENTES POR ESTADO =====");
        Console.WriteLine($"{"Estado",-8} {"Clientes",-10} {"Com Pedidos",-12} {"Valor Total",-15}");
        Console.WriteLine(new string('-', 50));

        foreach (var item in clientesPorEstado)
        {
            Console.WriteLine($"{item.Estado,-8} {item.QuantidadeClientes,-10} {item.ClientesComPedidos,-12} R$ {item.ValorTotalPedidos,-12:F2}");
        }

        var clientesComCompras = this._context.Clientes
            .Where(c => c.Pedidos.Any(p => p.Status != StatusPedido.Cancelado))
            .Select(c => new
            {
                Cliente = c.Nome,
                Email = c.Email,
                ValorTotal = c.Pedidos.Where(p => p.Status != StatusPedido.Cancelado).Sum(p => (double)p.ValorTotal),
                QuantidadePedidos = c.Pedidos.Count(p => p.Status != StatusPedido.Cancelado)
            })
            .ToList();

        if (clientesComCompras.Any())
        {
            var valorMedioCliente = clientesComCompras.Average(c => c.ValorTotal);
            var pedidoMedioCliente = clientesComCompras.Average(c => c.QuantidadePedidos);

            Console.WriteLine();
            Console.WriteLine("===== ESTATÍSTICAS GERAIS =====");
            Console.WriteLine($"Total de clientes com compras: {clientesComCompras.Count}");
            Console.WriteLine($"Valor médio por cliente: R$ {valorMedioCliente:F2}");
            Console.WriteLine($"Pedidos médios por cliente: {pedidoMedioCliente:F1}");

            var topClientes = clientesComCompras
                .OrderByDescending(c => c.ValorTotal)
                .Take(5)
                .ToList();

            Console.WriteLine();
            Console.WriteLine("===== TOP 5 CLIENTES =====");
            Console.WriteLine($"{"Cliente",-25} {"Pedidos",-8} {"Valor Total",-12}");
            Console.WriteLine(new string('-', 50));

            foreach (var cliente in topClientes)
            {
                Console.WriteLine($"{cliente.Cliente,-25} {cliente.QuantidadePedidos,-8} R$ {cliente.ValorTotal,-9:F2}");
            }
        }
    }

    public void Dispose()
    {
        this._context?.Dispose();
    }

    public async Task InicializarBanco()
    {
        await this._context.Database.EnsureCreatedAsync();
        await this._context.Database.MigrateAsync();
    }
}
