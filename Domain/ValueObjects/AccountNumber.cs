namespace BancoKRT.Domain.ValueObjects;

public class AccountNumber
{
    public string Value { get; private set; }

    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Conta é obrigatória");
        
        if (value.Length > 20)
            throw new ArgumentException("Conta deve ter no máximo 20 caracteres");
        
        Value = value;
    }

    public string GetCompositeKey(AgencyNumber agency) => $"{agency.Value}#{Value}";

    public override bool Equals(object? obj)
    {
        return obj is AccountNumber other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}