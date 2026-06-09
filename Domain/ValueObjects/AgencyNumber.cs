namespace BancoKRT.Domain.ValueObjects;

public class AgencyNumber
{
    public string Value { get; private set; }

    public AgencyNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Agência é obrigatória");
        
        if (value.Length > 10)
            throw new ArgumentException("Agência deve ter no máximo 10 caracteres");
        
        Value = value;
    }

    public override bool Equals(object? obj)
    {
        return obj is AgencyNumber other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}