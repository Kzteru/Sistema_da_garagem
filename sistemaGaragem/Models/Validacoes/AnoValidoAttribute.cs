using System.ComponentModel.DataAnnotations;

namespace sistemaGaragem.Models.Validacoes;

/// <summary>
/// Aceita anos entre 1900 e o ano seguinte ao atual (modelo do ano que vem).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AnoValidoAttribute : ValidationAttribute
{
    public const int AnoMinimo = 1900;

    public static int AnoMaximo => DateTime.Today.Year + 1;

    public AnoValidoAttribute()
    {
        ErrorMessage = $"Informe um ano entre {AnoMinimo} e {AnoMaximo}.";
    }

    public override bool IsValid(object? value) =>
        value is int ano && ano >= AnoMinimo && ano <= AnoMaximo;
}
