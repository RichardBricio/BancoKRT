// Domain/Entities/AccountLimit.cs - Versão completa e funcionando
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Domain.Entities;

public class AccountLimit
{
    private decimal _pixLimit;
    
    public Cpf Document { get; private set; }
    public AgencyNumber AgencyNumber { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public string PK => $"{AgencyNumber.Value}#{AccountNumber.Value}";
    public string GSI_PK => $"DOC#{Document.Value}";

    // Construtor para criação de NOVA conta
    public AccountLimit(Cpf document, AgencyNumber agency, AccountNumber account, decimal pixLimit)
    {
        Document = document;
        AgencyNumber = agency;
        AccountNumber = account;
        _pixLimit = pixLimit;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateLimit();
    }

    // Construtor para CARREGAR conta existente do banco
    public AccountLimit(Cpf document, AgencyNumber agency, AccountNumber account, 
                        decimal pixLimit, DateTime createdAt, DateTime updatedAt)
    {
        Document = document;
        AgencyNumber = agency;
        AccountNumber = account;
        _pixLimit = pixLimit;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private void ValidateLimit()
    {
        if (_pixLimit < 0)
            throw new DomainException("O limite não pode ser negativo");
    }

    public decimal GetLimit() => _pixLimit;

    public void UpdateLimit(decimal newLimit)
    {
        if (newLimit < 0)
            throw new DomainException("O limite não pode ser negativo");
        
        _pixLimit = newLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanTransact(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("O valor da transação deve ser maior que zero");
        
        return _pixLimit >= amount;
    }

    public void ConsumeLimit(decimal amount)
    {
        if (!CanTransact(amount))
            throw new DomainException($"Limite insuficiente. Disponível: R$ {_pixLimit:F2}, Necessário: R$ {amount:F2}");
        
        _pixLimit -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevertConsumption(decimal amount)
    {
        _pixLimit += amount;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}