using BancoKRT.Domain.Entities;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Domain.Interfaces;

public interface IAccountRepository
{
    Task<AccountLimit?> GetByAgencyAndAccountAsync(AgencyNumber agency, AccountNumber account);
    Task<IEnumerable<AccountLimit>> GetByDocumentAsync(Cpf document);
    Task<IEnumerable<AccountLimit>> GetAllAsync();
    Task AddAsync(AccountLimit account);
    Task UpdateAsync(AccountLimit account);
    Task DeleteAsync(AgencyNumber agency, AccountNumber account);
    Task<bool> ExistsAsync(AgencyNumber agency, AccountNumber account);
}