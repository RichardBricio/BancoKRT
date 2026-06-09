using BancoKRT.Domain.Entities;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Domain.Services;

public class PixDomainService
{
    private readonly IAccountRepository _repository;

    public PixDomainService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<(bool approved, decimal remainingLimit, string message)> ProcessPixTransactionAsync(
        AgencyNumber agency, AccountNumber account, decimal amount)
    {
        // Busca a conta
        var accountLimit = await _repository.GetByAgencyAndAccountAsync(agency, account);
        
        if (accountLimit == null)
        {
            return (false, 0, $"❌ Conta {agency.Value}/{account.Value} não encontrada!");
        }

        // Valida se pode transacionar (regra de negócio na entidade)
        if (!accountLimit.CanTransact(amount))
        {
            return (false, accountLimit.GetLimit(), 
                $"❌ Limite insuficiente! Disponível: R$ {accountLimit.GetLimit():F2}, Solicitado: R$ {amount:F2}");
        }

        // Consome o limite (regra de negócio na entidade)
        accountLimit.ConsumeLimit(amount);
        
        // Persiste a alteração
        await _repository.UpdateAsync(accountLimit);
        
        return (true, accountLimit.GetLimit(), 
            $"✅ Transação aprovada! Novo limite: R$ {accountLimit.GetLimit():F2}");
    }
}