using BancoKRT.Application.Services;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.Services;
using BancoKRT.Infrastructure.Repositories;
using BancoKRT.Domain.ValueObjects;
using BancoKRT.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BancoKRT;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     BANCO KRT - SISTEMA DE GESTÃO DE LIMITES PIX        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        
        // Configuração de injeção de dependência (SOLID - DIP)
        var services = new ServiceCollection();
        services.AddSingleton<IAccountRepository, DynamoDBRepository>();
        services.AddSingleton<PixDomainService>();
        services.AddSingleton<AccountAppService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var appService = serviceProvider.GetRequiredService<AccountAppService>();
        
        int option;
        do
        {
            Console.WriteLine("\n┌────────────────────────────────────────────────────────┐");
            Console.WriteLine("│                    MENU PRINCIPAL                       │");
            Console.WriteLine("├────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ 1 │ CADASTRAR limite                                   │");
            Console.WriteLine("│ 2 │ BUSCAR por Agência/Conta                           │");
            Console.WriteLine("│ 3 │ BUSCAR por CPF                                     │");
            Console.WriteLine("│ 4 │ ALTERAR limite                                     │");
            Console.WriteLine("│ 5 │ REMOVER registro                                   │");
            Console.WriteLine("│ 6 │ VALIDAR transação PIX                              │");
            Console.WriteLine("│ 7 │ LISTAR todos os registros                          │");
            Console.WriteLine("│ 8 │ RECRIAR dados de exemplo                           │");
            Console.WriteLine("│ 0 │ SAIR                                               │");
            Console.WriteLine("└────────────────────────────────────────────────────────┘");
            Console.Write("\n👉 Escolha uma opção: ");
            
            if (!int.TryParse(Console.ReadLine(), out option))
            {
                Console.WriteLine("❌ Opção inválida!");
                continue;
            }
            
            switch (option)
            {
                case 1: await CadastrarLimite(appService); break;
                case 2: await BuscarPorAgenciaConta(appService); break;
                case 3: await BuscarPorCpf(appService); break;
                case 4: await AlterarLimite(appService); break;
                case 5: await RemoverRegistro(appService); break;
                case 6: await ValidarTransacao(appService); break;
                case 7: await ListarTodos(appService); break;
                case 8: await RecriarDadosExemplo(appService); break;
                case 0: Console.WriteLine("\n👋 Saindo..."); break;
                default: Console.WriteLine("❌ Opção inválida!"); break;
            }
            
        } while (option != 0);
    }

    static async Task CadastrarLimite(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n📝 CADASTRO DE LIMITE PIX");
        Console.WriteLine("────────────────────────────────────────");
        
        Console.Write("CPF (11 dígitos): ");
        string cpf = Console.ReadLine() ?? "";
        
        Console.Write("Número da Agência: ");
        string agency = Console.ReadLine() ?? "";
        
        Console.Write("Número da Conta: ");
        string account = Console.ReadLine() ?? "";
        
        Console.Write("Limite PIX (R$): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal limit))
        {
            Console.WriteLine("❌ Valor inválido!");
            return;
        }
        
        await service.CreateAccountAsync(cpf, agency, account, limit);
        
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }

    static async Task BuscarPorAgenciaConta(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n🔍 BUSCAR POR AGÊNCIA/CONTA");
        Console.WriteLine("────────────────────────────────────────");
        
        Console.Write("Agência: ");
        string agency = Console.ReadLine() ?? "";
        
        Console.Write("Conta: ");
        string account = Console.ReadLine() ?? "";
        
        var result = await service.GetAccountAsync(agency, account);
        
        if (result == null)
        {
            Console.WriteLine("\n❌ Nenhum registro encontrado!");
        }
        else
        {
            Console.WriteLine("\n✅ REGISTRO ENCONTRADO:");
            Console.WriteLine($"   CPF: {result.Document}");
            Console.WriteLine($"   Agência: {result.AgencyNumber}");
            Console.WriteLine($"   Conta: {result.AccountNumber}");
            Console.WriteLine($"   Limite: R$ {result.GetLimit():F2}");
            Console.WriteLine($"   Criado em: {result.CreatedAt}");
            Console.WriteLine($"   Atualizado em: {result.UpdatedAt}");
        }
        
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }

    static async Task BuscarPorCpf(AccountAppService service)
{
    Console.Clear();
    Console.WriteLine("\n🔍 BUSCAR POR CPF");
    Console.WriteLine("────────────────────────────────────────");
    
    Console.Write("CPF (11 dígitos): ");
    string cpf = Console.ReadLine() ?? "";
    
    // Remove caracteres não numéricos se houver
    cpf = new string(cpf.Where(char.IsDigit).ToArray());
    
    if (cpf.Length != 11)
    {
        Console.WriteLine("\n❌ CPF inválido! Deve conter 11 dígitos.");
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
        return;
    }
    
    try
    {
        // CORRIGIDO: método correto é GetAccountsByCpfAsync
        var results = await service.GetAccountsByCpfAsync(cpf);
        
        if (!results.Any())
        {
            Console.WriteLine($"\n❌ Nenhum registro encontrado para o CPF: {cpf}");
        }
        else
        {
            Console.WriteLine($"\n✅ Encontrado(s) {results.Count()} registro(s) para o CPF: {cpf}\n");
            
            int count = 1;
            foreach (var item in results)
            {
                Console.WriteLine($"┌────────────────────────────────────────────┐");
                Console.WriteLine($"│ CONTA #{count}");
                Console.WriteLine($"├────────────────────────────────────────────┤");
                Console.WriteLine($"│ CPF: {item.Document}");
                Console.WriteLine($"│ Agência: {item.AgencyNumber}");
                Console.WriteLine($"│ Conta: {item.AccountNumber}");
                Console.WriteLine($"│ Limite: R$ {item.GetLimit():F2}");
                Console.WriteLine($"│ Criado em: {item.CreatedAt:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"│ Atualizado em: {item.UpdatedAt:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"└────────────────────────────────────────────┘");
                Console.WriteLine();
                count++;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Erro ao buscar: {ex.Message}");
    }
    
    Console.WriteLine("Pressione ENTER para continuar...");
    Console.ReadLine();
}

    static async Task AlterarLimite(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n✏️ ALTERAR LIMITE PIX");
        Console.WriteLine("────────────────────────────────────────");
        
        Console.Write("Agência: ");
        string agency = Console.ReadLine() ?? "";
        
        Console.Write("Conta: ");
        string account = Console.ReadLine() ?? "";
        
        Console.Write("Novo Limite (R$): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal newLimit))
        {
            Console.WriteLine("❌ Valor inválido!");
            return;
        }
        
        await service.UpdateLimitAsync(agency, account, newLimit);
        
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }

    static async Task RemoverRegistro(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n🗑️ REMOVER REGISTRO");
        Console.WriteLine("────────────────────────────────────────");
        
        Console.Write("Agência: ");
        string agency = Console.ReadLine() ?? "";
        
        Console.Write("Conta: ");
        string account = Console.ReadLine() ?? "";
        
        Console.Write("\n⚠️ Tem certeza? (S/N): ");
        string confirm = Console.ReadLine() ?? "";
        
        if (confirm.ToUpper() == "S")
        {
            await service.DeleteAccountAsync(agency, account);
        }
        else
        {
            Console.WriteLine("\n❌ Operação cancelada!");
        }
        
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }

    static async Task ValidarTransacao(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n💸 VALIDAR TRANSAÇÃO PIX");
        Console.WriteLine("────────────────────────────────────────");
        
        Console.Write("Agência de origem: ");
        string agency = Console.ReadLine() ?? "";
        
        Console.Write("Conta de origem: ");
        string account = Console.ReadLine() ?? "";
        
        Console.Write("Valor da transação (R$): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
        {
            Console.WriteLine("❌ Valor inválido!");
            return;
        }
        
        var result = await service.ValidatePixAsync(agency, account, amount);
        
        Console.WriteLine($"\n📊 RESULTADO:");
        Console.WriteLine(result.message);
        
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }

    static async Task ListarTodos(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n📋 LISTA DE TODOS OS LIMITES CADASTRADOS");
        Console.WriteLine("────────────────────────────────────────");
        
        var results = await service.GetAllAccountsAsync();
        
        if (!results.Any())
        {
            Console.WriteLine("\n❌ Nenhum registro encontrado!");
        }
        else
        {
            Console.WriteLine($"\n✅ Total de registros: {results.Count()}\n");
            foreach (var item in results)
            {
                Console.WriteLine($"┌────────────────────────────────────────────┐");
                Console.WriteLine($"│ Agência/Conta: {item.AgencyNumber}/{item.AccountNumber}");
                Console.WriteLine($"│ CPF: {item.Document}");
                Console.WriteLine($"│ Limite: R$ {item.GetLimit():F2}");
                Console.WriteLine($"│ Criado: {item.CreatedAt:dd/MM/yyyy HH:mm}");
                Console.WriteLine($"└────────────────────────────────────────────┘");
            }
        }
        
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }

    static async Task RecriarDadosExemplo(AccountAppService service)
    {
        Console.Clear();
        Console.WriteLine("\n🔄 RECRIANDO DADOS DE EXEMPLO");
        Console.WriteLine("────────────────────────────────────────");
        
        // Remove todos
        var existentes = await service.GetAllAccountsAsync();
        foreach (var item in existentes)
        {
            await service.DeleteAccountAsync(item.AgencyNumber.ToString(), item.AccountNumber.ToString());
        }
        
        Console.WriteLine("✅ Todos os registros removidos!\n");
        
        // Insere novos
        var contas = new (string cpf, string agencia, string conta, decimal limite)[]
        {
            ("11122233344", "0001", "10001", 5000m),
            ("11122233344", "0001", "10002", 3000m),
            ("22233344455", "0001", "10003", 10000m),
            ("22233344455", "0002", "20001", 7500m),
            ("33344455566", "0002", "20002", 2000m),
            ("33344455566", "0003", "30001", 15000m),
            ("44455566677", "0003", "30002", 8000m),
            ("44455566677", "0004", "40001", 4500m),
            ("55566677788", "0004", "40002", 12000m),
            ("55566677788", "0005", "50001", 6000m),
            ("66677788899", "0005", "50002", 3500m),
            ("66677788899", "0006", "60001", 9000m),
            ("77788899900", "0006", "60002", 2500m),
            ("77788899900", "0007", "70001", 11000m),
            ("88899900011", "0007", "70002", 4200m),
            ("88899900011", "0008", "80001", 7800m),
            ("99900011122", "0008", "80002", 5300m),
            ("99900011122", "0009", "90001", 20000m),
            ("12345678901", "0009", "90002", 1500m),
            ("98765432109", "0010", "00001", 30000m)
        };
        
        foreach (var conta in contas)
        {
            await service.CreateAccountAsync(conta.cpf, conta.agencia, conta.conta, conta.limite);
        }
        
        Console.WriteLine("\n📊 20 registros inseridos com sucesso!");
        Console.WriteLine("\nPressione ENTER para continuar...");
        Console.ReadLine();
    }
}