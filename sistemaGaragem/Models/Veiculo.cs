using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using sistemaGaragem.Models.Validacoes;

namespace sistemaGaragem.Models;

/// <summary>
/// Model: dados do veículo e validações dos campos.
/// </summary>
public class Veiculo
{
    public int Id { get; set; }

    private string _placa = string.Empty;

    [Required(ErrorMessage = "Informe a placa.")]
    [RegularExpression(@"^[A-Z]{3}-?\d[A-Z\d]\d{2}$",
        ErrorMessage = "Placa inválida. Use o padrão antigo (ABC-1234) ou Mercosul (ABC1D23).")]
    public string Placa
    {
        get => _placa;
        set => _placa = NormalizarPlaca(value);
    }

    [Required(ErrorMessage = "Informe a marca.")]
    [StringLength(50, ErrorMessage = "A marca deve ter no máximo 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o modelo.")]
    [StringLength(50, ErrorMessage = "O modelo deve ter no máximo 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [AnoValido]
    public int Ano { get; set; }

    [StringLength(30, ErrorMessage = "A cor deve ter no máximo 30 caracteres.")]
    public string? Cor { get; set; }

    /// <summary>Placa sem hífen, usada para comparar placas duplicadas.</summary>
    [JsonIgnore]
    public string PlacaSemHifen => Placa.Replace("-", string.Empty);

    /// <summary>Texto curto para listas e selects: "ABC1D23 — Fiat Argo".</summary>
    [JsonIgnore]
    public string Descricao => $"{Placa} — {Marca} {Modelo}";

    /// <summary>
    /// Deixa a placa em maiúsculas e sem espaços.
    /// Placa no padrão antigo (ABC1234) ganha o hífen: ABC-1234.
    /// </summary>
    private static string NormalizarPlaca(string? valor)
    {
        var placa = new string((valor ?? string.Empty)
            .Where(c => !char.IsWhiteSpace(c) && c != '-')
            .ToArray())
            .ToUpperInvariant();

        var padraoAntigo = placa.Length == 7
                           && placa[..3].All(char.IsLetter)
                           && placa[3..].All(char.IsDigit);

        return padraoAntigo ? $"{placa[..3]}-{placa[3..]}" : placa;
    }
}
