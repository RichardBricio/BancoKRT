using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Domain.Interfaces;

public interface IPixValidator
{
    Task<(bool approved, decimal remainingLimit, string message)> ValidateTransactionAsync(
        AgencyNumber agency, AccountNumber account, decimal amount);
}