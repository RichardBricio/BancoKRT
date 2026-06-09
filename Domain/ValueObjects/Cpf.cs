using System.Text.RegularExpressions;

namespace BancoKRT.Domain.ValueObjects;

public class Cpf
{
    public string Value { get; private set; }

    public Cpf(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("CPF inválido. Deve conter 11 dígitos.");
        
        Value = value;
    }

    private bool IsValid(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        
        // Remove caracteres não numéricos
        cpf = Regex.Replace(cpf, @"[^\d]", "");
        
        if (cpf.Length != 11) return false;
        
        // Verifica se todos os dígitos são iguais (CPF inválido)
        if (new string(cpf[0], 11) == cpf) return false;
        
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Cpf other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}