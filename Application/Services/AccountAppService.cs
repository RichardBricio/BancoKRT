using BancoKRT.Domain.Entities;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.Services;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Application.Services;

public class AccountAppService
{
    private readonly IAccountRepository _repository;
    private readonly PixDomainService _pixService;

    public AccountAppService(IAccountRepository repository, PixDomainService pixService)
    {
        _repository = repository;
        _pixService = pixService;
    }

    public async Task<bool> CreateAccountAsync(string cpf, string agency, string account, decimal limit)
    {
        try
        {
            var cpfVO = new Cpf(cpf);
            var agencyVO = new AgencyNumber(agency);
            var accountVO = new AccountNumber(account);
            
            if (await _repository.ExistsAsync(agencyVO, accountVO))
            {
                Console.WriteLine("❌ Conta já possui limite cadastrado!");
                return false;
            }
            
            var accountLimit = new AccountLimit(cpfVO, agencyVO, accountVO, limit);
            await _repository.AddAsync(accountLimit);
            
            Console.WriteLine($"✅ Limite cadastrado: R$ {limit:F2}");
            return true;
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
            return false;
        }
    }

    public async Task<AccountLimit?> GetAccountAsync(string agency, string account)
    {
        return await _repository.GetByAgencyAndAccountAsync(
            new AgencyNumber(agency), 
            new AccountNumber(account)
        );
    }

    public async Task<bool> UpdateLimitAsync(string agency, string account, decimal newLimit)
    {
        var accountLimit = await GetAccountAsync(agency, account);
        
        if (accountLimit == null)
        {
            Console.WriteLine("❌ Conta não encontrada!");
            return false;
        }
        
        try
        {
            accountLimit.UpdateLimit(newLimit);
            await _repository.UpdateAsync(accountLimit);
            Console.WriteLine($"✅ Limite alterado: R$ {newLimit:F2}");
            return true;
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(string agency, string account)
    {
        var accountLimit = await GetAccountAsync(agency, account);
        
        if (accountLimit == null)
        {
            Console.WriteLine("❌ Conta não encontrada!");
            return false;
        }
        
        await _repository.DeleteAsync(
            new AgencyNumber(agency), 
            new AccountNumber(account)
        );
        
        Console.WriteLine("✅ Registro removido!");
        return true;
    }

    public async Task<(bool approved, decimal remainingLimit, string message)> ValidatePixAsync(
        string agency, string account, decimal amount)
    {
        return await _pixService.ProcessPixTransactionAsync(
            new AgencyNumber(agency),
            new AccountNumber(account),
            amount
        );
    }

    public async Task<IEnumerable<AccountLimit>> GetAllAccountsAsync()
    {
        return await _repository.GetAllAsync();
    }

    // Application/Services/AccountAppService.cs
// Adicione este método

// Application/Services/AccountAppService.cs
// ALTERE este método para retornar uma LISTA

public async Task<IEnumerable<AccountLimit>> GetAccountsByCpfAsync(string cpf)
{
    try
    {
        var cpfVO = new Cpf(cpf);
        return await _repository.GetByDocumentAsync(cpfVO);
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"❌ {ex.Message}");
        return new List<AccountLimit>();
    }
}
}