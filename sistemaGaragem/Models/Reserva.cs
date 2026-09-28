using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace sistemaGaragem.Models;

/// <summary>
/// Model: vincula um veículo a uma pessoa durante um período (datas inclusivas).
/// </summary>
public class Reserva : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Veículo")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um veículo.")]
    public int VeiculoId { get; set; }

    [Display(Name = "Pessoa")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma pessoa.")]
    public int PessoaId { get; set; }

    private DateTime _dataInicio;
    private DateTime _dataFim;

    [Display(Name = "Data de início")]
    [DataType(DataType.Date)]
    public DateTime DataInicio
    {
        get => _dataInicio;
        set => _dataInicio = value.Date;
    }

    [Display(Name = "Data de fim")]
    [DataType(DataType.Date)]
    public DateTime DataFim
    {
        get => _dataFim;
        set => _dataFim = value.Date;
    }

    /// <summary>
    /// Regra de sobreposição de períodos:
    /// novaInicio &lt;= existenteFim E novaFim &gt;= existenteInicio
    /// </summary>
    public bool SobrepoeA(DateTime inicio, DateTime fim) =>
        inicio.Date <= DataFim && fim.Date >= DataInicio;

    /// <summary>Indica se a reserva cobre o dia informado.</summary>
    public bool Abrange(DateTime dia) => SobrepoeA(dia, dia);

    /// <summary>"Futura", "Em andamento" ou "Encerrada" em relação ao dia informado.</summary>
    public string SituacaoEm(DateTime dia)
    {
        if (dia.Date < DataInicio) return "Futura";
        if (dia.Date > DataFim) return "Encerrada";
        return "Em andamento";
    }

    [JsonIgnore]
    public int QuantidadeDias => (DataFim - DataInicio).Days + 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataFim < DataInicio)
        {
            yield return new ValidationResult(
                "A data de fim não pode ser anterior à data de início.",
                [nameof(DataFim)]);
        }
    }
}
