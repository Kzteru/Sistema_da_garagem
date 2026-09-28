using System.ComponentModel.DataAnnotations;

namespace sistemaGaragem.Models.Validacoes;

/// <summary>
/// Valida os dígitos verificadores do CPF.
/// Valor vazio é aceito aqui: quem obriga o preenchimento é o [Required].
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CpfAttribute : ValidationAttribute
{
    public CpfAttribute()
    {
        ErrorMessage = "CPF inválido.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not string texto || string.IsNullOrWhiteSpace(texto))
            return true;

        return EhValido(texto);
    }

    public static string SomenteDigitos(string? texto) =>
        new((texto ?? string.Empty).Where(char.IsDigit).ToArray());

    public static bool EhValido(string? cpf)
    {
        var d = SomenteDigitos(cpf);

        if (d.Length != 11 || d.Distinct().Count() == 1)
            return false;

        for (var tamanho = 9; tamanho < 11; tamanho++)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++)
                soma += (d[i] - '0') * (tamanho + 1 - i);

            var digito = soma * 10 % 11;
            if (digito == 10) digito = 0;

            if (digito != d[tamanho] - '0')
                return false;
        }

        return true;
    }
}
