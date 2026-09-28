namespace sistemaGaragem.ViewModels;

/// <summary>Uma linha da listagem de reservas, já com placa, modelo e nome da pessoa.</summary>
public class ReservaItemViewModel
{
    public int Id { get; init; }
    public string Placa { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public string NomePessoa { get; init; } = string.Empty;
    public DateTime DataInicio { get; init; }
    public DateTime DataFim { get; init; }
    public int QuantidadeDias { get; init; }
    public string Situacao { get; init; } = string.Empty;
}
