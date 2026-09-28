using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using sistemaGaragem.Models.Validacoes;

namespace sistemaGaragem.Models;

/// <summary>
/// Model: guarda os dados da pessoa e as regras de validação dos campos.
/// Não sabe nada sobre JSON, arquivos ou telas.
/// </summary>
public class Pessoa
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    private string _cpf = string.Empty;

    [Display(Name = "CPF")]
    [Required(ErrorMessage = "Informe o CPF.")]
    [Cpf]
    public string Cpf
    {
        get => _cpf;
        set => _cpf = FormatarCpf(value);
    }

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Informe um e-mail em formato válido (ex.: nome@dominio.com).")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^[\d\s()+-]{8,20}$", ErrorMessage = "Telefone inválido. Use apenas números, espaços, parênteses e hífen.")]
    [DataType(DataType.PhoneNumber)]
    public string? Telefone { get; set; }

    /// <summary>CPF só com números, usado para comparar CPFs duplicados.</summary>
    [JsonIgnore]
    public string CpfSomenteDigitos => CpfAttribute.SomenteDigitos(Cpf);

    /// <summary>Se o CPF vier com 11 dígitos (com ou sem máscara), grava no formato 000.000.000-00.</summary>
    private static string FormatarCpf(string? valor)
    {
        var texto = valor?.Trim() ?? string.Empty;
        var d = CpfAttribute.SomenteDigitos(texto);

        return d.Length == 11
            ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}"
            : texto;
    }
}
