using System.Data.SQLite;

namespace CheckPoint1.Services;

public class AdoNetService
{
    private readonly string _connectionString;

    public AdoNetService()
    {
        // Connection string para SQLite - mesmo arquivo usado pelo EF
        this._connectionString = "Data Source=loja.db";
    }

    // ========== CONSULTAS COMPLEXAS ==========

    public void RelatorioVendasCompleto()
    {
        Console.WriteLine("=== RELATÓRIO VENDAS COMPLETO (ADO.NET) ===");

        using var connection = GetConnection();
        connection.Open();

        var query = @"
            SELECT 
                p.NumeroPedido,
                c.Nome as NomeCliente,
                pr.Nome as NomeProduto,
                pi.Quantidade,
                pi.PrecoUnitario,
                (pi.Quantidade * pi.PrecoUnitario - COALESCE(pi.Desconto, 0)) as Subtotal,
                p.DataPedido,
                p.Status
            FROM Pedidos p
            INNER JOIN Clientes c ON p.ClienteId = c.Id
            INNER JOIN PedidoItens pi ON p.Id = pi.PedidoId
            INNER JOIN Produtos pr ON pi.ProdutoId = pr.Id
            WHERE p.Status != 5  -- Não cancelado
            ORDER BY p.DataPedido DESC, p.NumeroPedido, pr.Nome";

        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();

        Console.WriteLine();
        Console.WriteLine($"{"Pedido",-12} {"Cliente",-20} {"Produto",-25} {"Qtd",-5} {"Preço",-10} {"Subtotal",-10} {"Data",-12}");
        Console.WriteLine(new string('-', 100));

        var pedidoAtual = "";
        var totalPedido = 0.0;

        while (reader.Read())
        {
            var numeroPedido = reader["NumeroPedido"].ToString() ?? "";
            var nomeCliente = reader["NomeCliente"].ToString() ?? "";
            var nomeProduto = reader["NomeProduto"].ToString() ?? "";
            var quantidade = Convert.ToInt32(reader["Quantidade"]);
            var precoUnitario = Convert.ToDecimal(reader["PrecoUnitario"]);
            var subtotal = Convert.ToDouble(reader["Subtotal"]);
            var dataPedido = Convert.ToDateTime(reader["DataPedido"]);

            if (pedidoAtual != numeroPedido)
            {
                if (!string.IsNullOrEmpty(pedidoAtual))
                {
                    Console.WriteLine($"{"TOTAL PEDIDO:",-70} R$ {totalPedido:F2}");
                    Console.WriteLine(new string('-', 100));
                }
                pedidoAtual = numeroPedido;
                totalPedido = 0;
            }

            Console.WriteLine($"{numeroPedido,-12} {nomeCliente,-20} {nomeProduto,-25} {quantidade,-5} R$ {precoUnitario,-7:F2} R$ {subtotal,-7:F2} {dataPedido:dd/MM/yyyy}");
            totalPedido += subtotal;
        }

        if (!string.IsNullOrEmpty(pedidoAtual))
        {
            Console.WriteLine($"{"TOTAL PEDIDO:",-70} R$ {totalPedido:F2}");
        }
    }
    public void FaturamentoPorCliente()
    {

        Console.WriteLine("=== FATURAMENTO POR CLIENTE ===");
        var connection = GetConnection();
        connection.Open();
        var query = @"
            SELECT 
                c.Nome as Cliente,
                c.Email,
                SUM(p.ValorTotal) as ValorTotalPedidos,
                COUNT(p.Id) as QuantidadePedidos,
                AVG(p.ValorTotal) as TicketMedio
            FROM Clientes c
            LEFT JOIN Pedidos p ON c.Id = p.ClienteId AND p.Status != 5  -- Não cancelado
            GROUP BY c.Id, c.Nome, c.Email
            HAVING COUNT(p.Id) > 0
            ORDER BY ValorTotalPedidos DESC, c.Nome";
        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();
        Console.WriteLine();
        Console.WriteLine($"{"Cliente",-25} {"Email",-30} {"Total Pedidos",-15} {"Qtd Pedidos",-12} {"Ticket Médio",-12}");
        while (reader.Read())
        {
            var cliente = reader["Cliente"].ToString() ?? "";
            var email = reader["Email"].ToString() ?? "";
            var valorTotal = Convert.ToDecimal(reader["ValorTotalPedidos"]);
            var quantidadePedidos = Convert.ToInt32(reader["QuantidadePedidos"]);
            var ticketMedio = Convert.ToDecimal(reader["TicketMedio"]);
            Console.WriteLine($"{cliente,-25} {email,-30} R$ {valorTotal,-12:F2} {quantidadePedidos,-12} R$ {ticketMedio,-9:F2}");
        }
    }

    public void ProdutosSemVenda()
    {
        Console.WriteLine("=== PRODUTOS SEM VENDAS ===");

        using var connection = GetConnection();
        connection.Open();

        var query = @"
            SELECT 
                p.Id,
                p.Nome,
                c.Nome as Categoria,
                p.Preco,
                p.Estoque,
                (p.Preco * p.Estoque) as ValorParado
            FROM Produtos p
            INNER JOIN Categorias c ON p.CategoriaId = c.Id
            LEFT JOIN PedidoItens pi ON p.Id = pi.ProdutoId
            WHERE pi.ProdutoId IS NULL
            AND p.Ativo = 1
            ORDER BY ValorParado DESC, p.Nome";

        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();

        Console.WriteLine();
        Console.WriteLine($"{"Produto",-30} {"Categoria",-15} {"Preço",-10} {"Estoque",-8} {"Valor Parado",-12}");
        Console.WriteLine(new string('-', 80));

        var valorTotalParado = 0.0;
        var totalProdutos = 0;

        while (reader.Read())
        {
            var nome = reader["Nome"].ToString() ?? "";
            var categoria = reader["Categoria"].ToString() ?? "";
            var preco = Convert.ToDouble(reader["Preco"]);
            var estoque = Convert.ToInt32(reader["Estoque"]);
            var valorParado = Convert.ToDouble(reader["ValorParado"]);

            Console.WriteLine($"{nome,-30} {categoria,-15} R$ {preco,-7:F2} {estoque,-8} R$ {valorParado,-9:F2}");

            valorTotalParado += valorParado;
            totalProdutos++;
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total de produtos sem venda: {totalProdutos}");
        Console.WriteLine($"Valor total parado em estoque: R$ {valorTotalParado:F2}");

        if (totalProdutos == 0)
        {
            Console.WriteLine("Todos os produtos já foram vendidos pelo menos uma vez!");
        }
    }

    // ========== OPERAÇÕES DE DADOS ==========
    public void AtualizarEstoqueLote()
    {
        Console.WriteLine("=== ATUALIZAR ESTOQUE EM LOTE ===");

        using var connection = GetConnection();
        connection.Open();

        var queryCategorias = "SELECT Id, Nome FROM Categorias ORDER BY Id";
        using var commandCategorias = new SQLiteCommand(queryCategorias, connection);
        using var readerCategorias = commandCategorias.ExecuteReader();

        Console.WriteLine("Categorias disponíveis:");
        while (readerCategorias.Read())
        {
            var id = Convert.ToInt32(readerCategorias["Id"]);
            var nome = readerCategorias["Nome"].ToString() ?? "";
            Console.WriteLine($"{id} - {nome}");
        }
        readerCategorias.Close();

        Console.Write("Digite o Id da categoria: ");
        if (!int.TryParse(Console.ReadLine(), out var categoriaId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var queryProdutos = @"
            SELECT p.Id, p.Nome, p.Estoque 
            FROM Produtos p 
            WHERE p.CategoriaId = @categoriaId AND p.Ativo = 1
            ORDER BY p.Nome";

        using var commandProdutos = new SQLiteCommand(queryProdutos, connection);
        commandProdutos.Parameters.AddWithValue("@categoriaId", categoriaId);
        using var readerProdutos = commandProdutos.ExecuteReader();

        var produtos = new List<(int Id, string Nome, int EstoqueAtual)>();
        while (readerProdutos.Read())
        {
            produtos.Add((
                Convert.ToInt32(readerProdutos["Id"]),
                readerProdutos["Nome"].ToString() ?? "",
                Convert.ToInt32(readerProdutos["Estoque"])
            ));
        }
        readerProdutos.Close();

        if (!produtos.Any())
        {
            Console.WriteLine("Nenhum produto encontrado nesta categoria.");
            return;
        }

        Console.WriteLine($"Produtos da categoria (Total: {produtos.Count}):");
        var registrosAfetados = 0;

        using var transaction = connection.BeginTransaction();

        foreach (var produto in produtos)
        {
            Console.WriteLine($"Produto: {produto.Nome} (Estoque atual: {produto.EstoqueAtual})");
            Console.Write("Nova quantidade: ");

            if (!int.TryParse(Console.ReadLine(), out var novaQuantidade) || novaQuantidade < 0)
            {
                Console.WriteLine("Quantidade inválida! Mantendo estoque atual.");
                continue;
            }

            var queryUpdate = "UPDATE Produtos SET Estoque = @estoque WHERE Id = @id";
            using var commandUpdate = new SQLiteCommand(queryUpdate, connection, transaction);
            commandUpdate.Parameters.AddWithValue("@estoque", novaQuantidade);
            commandUpdate.Parameters.AddWithValue("@id", produto.Id);

            var linhasAfetadas = commandUpdate.ExecuteNonQuery();
            if (linhasAfetadas > 0)
            {
                registrosAfetados++;
                Console.WriteLine($"✓ Estoque atualizado: {produto.EstoqueAtual} → {novaQuantidade}");
            }
        }

        transaction.Commit();
        Console.WriteLine($"Atualização concluída! {registrosAfetados} produtos atualizados.");
    }

    public void InserirPedidoCompleto()
    {
        Console.WriteLine("=== INSERIR PEDIDO COMPLETO ===");

        using var connection = GetConnection();
        connection.Open();

        var queryClientes = "SELECT Id, Nome, Email FROM Clientes WHERE Ativo = 1 ORDER BY Id";
        using var commandClientes = new SQLiteCommand(queryClientes, connection);
        using var readerClientes = commandClientes.ExecuteReader();

        Console.WriteLine("Clientes disponíveis:");
        while (readerClientes.Read())
        {
            var id = Convert.ToInt32(readerClientes["Id"]);
            var nome = readerClientes["Nome"].ToString() ?? "";
            var email = readerClientes["Email"].ToString() ?? "";
            Console.WriteLine($"{id} - {nome} ({email})");
        }
        readerClientes.Close();

        Console.Write("Digite o Id do cliente: ");
        if (!int.TryParse(Console.ReadLine(), out int clienteId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var queryValidarClientes = "SELECT COUNT(*) FROM Clientes WHERE Id = @id AND Ativo = 1";
        using var commandValidar = new SQLiteCommand(queryValidarClientes, connection);
        commandValidar.Parameters.AddWithValue("@id", clienteId);
        if (Convert.ToInt32(commandValidar.ExecuteScalar()) == 0)
        {
            Console.WriteLine("Cliente não encontrado ou inativo!");
            return;
        }

        var queryUltimoPedido = "SELECT COALESCE(MAX(Id), 0) + 1 FROM Pedidos";
        using var commandUltimo = new SQLiteCommand(queryUltimoPedido, connection);
        var proximoId = Convert.ToInt32(commandUltimo.ExecuteScalar());
        var numeroPedido = $"PED-{proximoId:D3}";

        using var transaction = connection.BeginTransaction();
        var queryInserirPedido = @"
            INSERT INTO Pedidos (NumeroPedido, DataPedido, Status, ValorTotal, ClienteId)
            VALUES (@numero, @data, @status, @valor, @clienteId)";

        using var commandPedido = new SQLiteCommand(queryInserirPedido, connection, transaction);
        commandPedido.Parameters.AddWithValue("@numero", numeroPedido);
        commandPedido.Parameters.AddWithValue("@data", DateTime.Now);
        commandPedido.Parameters.AddWithValue("@status", (int)StatusPedido.Pendente);
        commandPedido.Parameters.AddWithValue("@valor", 0);
        commandPedido.Parameters.AddWithValue("@clienteId", clienteId);

        commandPedido.ExecuteNonQuery();

        var querySelectPedidoById = "SELECT last_insert_rowid()";
        using var commandId = new SQLiteCommand(querySelectPedidoById, connection, transaction);
        var pedidoId = Convert.ToInt32(commandId.ExecuteScalar());

        Console.WriteLine($"Pedido {numeroPedido} criado! Adicione os itens:");

        var valorTotal = 0.0;
        var continuarAdicionando = true;

        while (continuarAdicionando)
        {
            var queryProdutos = @"
                SELECT p.Id, p.Nome, p.Preco, p.Estoque, c.Nome as Categoria
                FROM Produtos p
                INNER JOIN Categorias c ON p.CategoriaId = c.Id
                WHERE p.Ativo = 1 AND p.Estoque > 0
                ORDER BY p.Id";

            using var commandProdutos = new SQLiteCommand(queryProdutos, connection, transaction);
            using var readerProdutos = commandProdutos.ExecuteReader();

            Console.WriteLine("\nProdutos disponíveis:");
            while (readerProdutos.Read())
            {
                var id = Convert.ToInt32(readerProdutos["Id"]);
                var nome = readerProdutos["Nome"].ToString() ?? "";
                var preco = Convert.ToDouble(readerProdutos["Preco"]);
                var estoque = Convert.ToInt32(readerProdutos["Estoque"]);
                Console.WriteLine($"{id} - {nome} - R$ {preco:F2} (Estoque: {estoque})");
            }
            readerProdutos.Close();

            Console.Write("Digite o Id do produto (0 para finalizar): ");
            if (!int.TryParse(Console.ReadLine(), out int produtoId) || produtoId == 0)
            {
                break;
            }

            var queryValidarProduto = "SELECT Preco, Estoque FROM Produtos WHERE Id = @id AND Ativo = 1";
            using var commandValidarProd = new SQLiteCommand(queryValidarProduto, connection, transaction);
            commandValidarProd.Parameters.AddWithValue("@id", produtoId);
            using var readerValidar = commandValidarProd.ExecuteReader();

            if (!readerValidar.Read())
            {
                readerValidar.Close();
                Console.WriteLine("Produto não encontrado!");
                continue;
            }

            var precoProduto = Convert.ToDecimal(readerValidar["Preco"]);
            var estoqueProduto = Convert.ToInt32(readerValidar["Estoque"]);
            readerValidar.Close();

            Console.Write($"Quantidade (máx: {estoqueProduto}): ");
            if (!int.TryParse(Console.ReadLine(), out int quantidade) || quantidade <= 0)
            {
                Console.WriteLine("Quantidade inválida!");
                continue;
            }

            if (quantidade > estoqueProduto)
            {
                Console.WriteLine($"Estoque insuficiente! Disponível: {estoqueProduto}");
                continue;
            }

            var queryIserirItem = @"
                INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
                VALUES (@pedidoId, @produtoId, @quantidade, @preco)";

            using var commandItem = new SQLiteCommand(queryIserirItem, connection, transaction);
            commandItem.Parameters.AddWithValue("@pedidoId", pedidoId);
            commandItem.Parameters.AddWithValue("@produtoId", produtoId);
            commandItem.Parameters.AddWithValue("@quantidade", quantidade);
            commandItem.Parameters.AddWithValue("@preco", precoProduto);

            commandItem.ExecuteNonQuery();

            var queryAtualizarEstoque = "UPDATE Produtos SET Estoque = Estoque - @quantidade WHERE Id = @id";
            using var commandEstoque = new SQLiteCommand(queryAtualizarEstoque, connection, transaction);
            commandEstoque.Parameters.AddWithValue("@quantidade", quantidade);
            commandEstoque.Parameters.AddWithValue("@id", produtoId);

            commandEstoque.ExecuteNonQuery();

            var subtotal = (double)(quantidade * precoProduto);
            valorTotal += subtotal;

            Console.WriteLine($"Item adicionado! Subtotal: R$ {subtotal:F2}");

            Console.Write("Adicionar outro item? (S/N): ");
            continuarAdicionando = Console.ReadLine()?.ToUpper() == "S";
        }

        var queryAtualizarValor = "UPDATE Pedidos SET ValorTotal = @valor WHERE Id = @id";
        using var commandValor = new SQLiteCommand(queryAtualizarValor, connection, transaction);
        commandValor.Parameters.AddWithValue("@valor", valorTotal);
        commandValor.Parameters.AddWithValue("@id", pedidoId);

        commandValor.ExecuteNonQuery();

        transaction.Commit();
        Console.WriteLine($"Pedido {numeroPedido} finalizado com sucesso!");
        Console.WriteLine($"Valor total: R$ {valorTotal:F2}");
    }


    public void ExcluirDadosAntigos()
    {
        var connection = GetConnection();
        connection.Open();
        var dataLimite = DateTime.Now.AddMonths(-6);

        var query = "DELETE FROM Pedidos WHERE Status = 5 AND DataPedido < @dataLimite";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@dataLimite", dataLimite);
        var linhasAfetadas = command.ExecuteNonQuery();
        Console.WriteLine($"{linhasAfetadas} pedidos antigos excluídos.");

        Console.WriteLine("=== EXCLUIR DADOS ANTIGOS ===");
    }

    public void ProcessarDevolucao()
    {
        Console.WriteLine("=== PROCESSAR DEVOLUÇÃO ===");

        using var connection = GetConnection();
        connection.Open();

        var queryListarProdutos = @"
            SELECT p.Id, p.NumeroPedido, c.Nome as Cliente, p.DataPedido, p.Status, p.ValorTotal
            FROM Pedidos p
            INNER JOIN Clientes c ON p.ClienteId = c.Id
            WHERE p.Status = 4  -- Entregue
            ORDER BY p.DataPedido DESC";
        
        using var commandListar = new SQLiteCommand(queryListarProdutos, connection);
        using var readerListar = commandListar.ExecuteReader();
        Console.WriteLine();
        Console.WriteLine($"{"Id",-5} {"Pedido",-12} {"Cliente",-20} {"Data",-12} {"Status",-10} {"Valor",-10}");
        Console.WriteLine(new string('-', 70));
        while (readerListar.Read())
        {
            var id = Convert.ToInt32(readerListar["Id"]);
            var numeroPedidoListar = readerListar["NumeroPedido"].ToString() ?? "";
            var clientePedidoListar = readerListar["Cliente"].ToString() ?? "";
            var dataPedido = Convert.ToDateTime(readerListar["DataPedido"]);
            var statusPedidoListar = Convert.ToInt32(readerListar["Status"]);
            var valorPedidoListarTotal = Convert.ToDecimal(readerListar["ValorTotal"]);

            Console.WriteLine($"{id,-5} {numeroPedidoListar,-12} {numeroPedidoListar,-20} {dataPedido:dd/MM/yyyy,-12} {(StatusPedido)statusPedidoListar,-10} R$ {valorPedidoListarTotal,-7:F2}");
        }

        Console.Write("Digite o Id do pedido para devolução: ");
        if (!int.TryParse(Console.ReadLine(), out int pedidoId))
        {
            Console.WriteLine("Id inválido!");
            return;
        }

        var querySelectProduto = @"
            SELECT p.Id, p.NumeroPedido, p.Status, p.ValorTotal, c.Nome as Cliente
            FROM Pedidos p
            INNER JOIN Clientes c ON p.ClienteId = c.Id
            WHERE p.Id = @id";

        using var commandPedido = new SQLiteCommand(querySelectProduto, connection);
        commandPedido.Parameters.AddWithValue("@id", pedidoId);
        using var readerPedido = commandPedido.ExecuteReader();

        if (!readerPedido.Read())
        {
            readerPedido.Close();
            Console.WriteLine("Pedido não encontrado!");
            return;
        }

        var numeroPedido = readerPedido["NumeroPedido"].ToString() ?? "";
        var status = Convert.ToInt32(readerPedido["Status"]);
        var valorTotal = Convert.ToDecimal(readerPedido["ValorTotal"]);
        var cliente = readerPedido["Cliente"].ToString() ?? "";
        readerPedido.Close();

        Console.WriteLine($"Pedido: {numeroPedido}");
        Console.WriteLine($"Cliente: {cliente}");
        Console.WriteLine($"Status: {(StatusPedido)status}");
        Console.WriteLine($"Valor: R$ {valorTotal:F2}");

        if (status != (int)StatusPedido.Entregue)
        {
            Console.WriteLine("Apenas pedidos com status 'Entregue' podem ser devolvidos!");
            return;
        }

        var queryItens = @"
            SELECT pi.Id, pi.Quantidade, pi.PrecoUnitario, pr.Nome as Produto, pr.Id as ProdutoId
            FROM PedidoItens pi
            INNER JOIN Produtos pr ON pi.ProdutoId = pr.Id
            WHERE pi.PedidoId = @pedidoId";

        using var commandItens = new SQLiteCommand(queryItens, connection);
        commandItens.Parameters.AddWithValue("@pedidoId", pedidoId);
        using var readerItens = commandItens.ExecuteReader();

        var itens = new List<(int Id, int ProdutoId, string Produto, int Quantidade, decimal Preco)>();

        Console.WriteLine("\nItens do pedido:");
        while (readerItens.Read())
        {
            var itemId = Convert.ToInt32(readerItens["Id"]);
            var produtoId = Convert.ToInt32(readerItens["ProdutoId"]);
            var produto = readerItens["Produto"].ToString() ?? "";
            var quantidade = Convert.ToInt32(readerItens["Quantidade"]);
            var preco = Convert.ToDecimal(readerItens["PrecoUnitario"]);

            itens.Add((itemId, produtoId, produto, quantidade, preco));
            Console.WriteLine($"- {produto} x{quantidade} = R$ {(quantidade * preco):F2}");
        }
        readerItens.Close();

        Console.Write("\nConfirma a devolução completa? (S/N): ");
        if (Console.ReadLine()?.ToUpper() != "S")
        {
            Console.WriteLine("Devolução cancelada.");
            return;
        }

        using var transaction = connection.BeginTransaction();

        foreach (var item in itens)
        {
            var sqlDevolverEstoque = "UPDATE Produtos SET Estoque = Estoque + @quantidade WHERE Id = @produtoId";
            using var commandDevolver = new SQLiteCommand(sqlDevolverEstoque, connection, transaction);
            commandDevolver.Parameters.AddWithValue("@quantidade", item.Quantidade);
            commandDevolver.Parameters.AddWithValue("@produtoId", item.ProdutoId);

            commandDevolver.ExecuteNonQuery();
            Console.WriteLine($"Estoque devolvido: {item.Produto} +{item.Quantidade}");
        }

        var queryCancelarPedido = "UPDATE Pedidos SET Status = 5 WHERE Id = @id";
        using var commandCancelar = new SQLiteCommand(queryCancelarPedido, connection, transaction);
        commandCancelar.Parameters.AddWithValue("@id", pedidoId);

        commandCancelar.ExecuteNonQuery();

        transaction.Commit();

        Console.WriteLine($"\nDevolução processada com sucesso!");
        Console.WriteLine($"Pedido {numeroPedido} marcado como cancelado.");
        Console.WriteLine($"Estoque de {itens.Count} produtos foi restaurado.");
    }


    // ========== ANÁLISES PERFORMANCE ==========
    public void AnalisarPerformanceVendas()
    {
        Console.WriteLine("=== ANÁLISE PERFORMANCE VENDAS ===");

        using var connection = GetConnection();
        connection.Open();

        var query = @"
            SELECT 
                strftime('%Y', DataPedido) as Ano,
                strftime('%m', DataPedido) as Mes,
                COUNT(Id) as QuantidadePedidos,
                SUM(ValorTotal) as FaturamentoMensal,
                AVG(ValorTotal) as TicketMedio
            FROM Pedidos
            WHERE Status != 5  -- Não cancelado
            GROUP BY strftime('%Y-%m', DataPedido)
            ORDER BY Ano, Mes";

        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();

        Console.WriteLine();
        Console.WriteLine($"{"Período",-10} {"Pedidos",-8} {"Faturamento",-15} {"Ticket Médio",-12} {"Crescimento",-12}");
        Console.WriteLine(new string('-', 70));

        decimal? faturamentoAnterior = null;

        while (reader.Read())
        {
            var ano = reader["Ano"].ToString() ?? "";
            var mes = reader["Mes"].ToString() ?? "";
            var periodo = $"{mes}/{ano}";
            var quantidadePedidos = Convert.ToInt32(reader["QuantidadePedidos"]);
            var faturamentoMensal = Convert.ToDecimal(reader["FaturamentoMensal"]);
            var ticketMedio = Convert.ToDecimal(reader["TicketMedio"]);

            var crescimento = "N/A";
            if (faturamentoAnterior.HasValue && faturamentoAnterior > 0)
            {
                var percentual = ((faturamentoMensal - faturamentoAnterior.Value) / faturamentoAnterior.Value) * 100;
                crescimento = $"{percentual:+0.0;-0.0;0.0}%";
            }

            Console.WriteLine($"{periodo,-10} {quantidadePedidos,-8} R$ {faturamentoMensal,-12:F2} R$ {ticketMedio,-9:F2} {crescimento,-12}");
            faturamentoAnterior = faturamentoMensal;
        }
    }

    // ========== UTILIDADES ==========
    private SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(this._connectionString);
    }


    public void TestarConexao()
    {
        Console.WriteLine("=== TESTE DE CONEXÃO ===");

        using var connection = GetConnection();
        connection.Open();

        Console.WriteLine("✓ Conexão estabelecida com sucesso!");
        Console.WriteLine($"Banco de dados: {connection.DataSource}");
        Console.WriteLine($"Versão SQLite: {connection.ServerVersion}");

        var sqlTotalCategorias = "SELECT COUNT(*) as TotalCategorias FROM Categorias";
        using var command = new SQLiteCommand(sqlTotalCategorias, connection);
        var totalCategorias = Convert.ToInt32(command.ExecuteScalar());

        Console.WriteLine($"Total de categorias: {totalCategorias}");

        var queryTotalProdutos = "SELECT COUNT(*) as TotalProdutos FROM Produtos";
        using var commandProdutos = new SQLiteCommand(queryTotalProdutos, connection);
        var totalProdutos = Convert.ToInt32(commandProdutos.ExecuteScalar());
        Console.WriteLine($"Total de produtos: {totalProdutos}");

        var queryTotalClientes = "SELECT COUNT(*) as TotalClientes FROM Clientes";
        using var commandClientes = new SQLiteCommand(queryTotalClientes, connection);
        var totalClientes = Convert.ToInt32(commandClientes.ExecuteScalar());
        Console.WriteLine($"Total de clientes: {totalClientes}");

        var queryTabelas = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
        using var commandTabelas = new SQLiteCommand(queryTabelas, connection);
        using var reader = commandTabelas.ExecuteReader();

        Console.WriteLine("\nTabelas no banco:");
        while (reader.Read())
        {
            var nomeTabela = reader["name"].ToString() ?? "";
            Console.WriteLine($"- {nomeTabela}");
        }
    }
}
