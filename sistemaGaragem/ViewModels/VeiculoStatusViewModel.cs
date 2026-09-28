using sistemaGaragem.Models;

namespace sistemaGaragem.ViewModels;

/// <summary>Veículo + status calculado a partir das reservas (Disponível / Reservado hoje).</summary>
public class VeiculoStatusViewModel
{
    public required Veiculo Veiculo { get; init; }
    public bool Disponivel { get; init; }
}
