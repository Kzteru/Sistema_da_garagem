using sistemaGaragem.Models;

namespace sistemaGaragem.ViewModels;

/// <summary>Formulário de edição do período de uma reserva.</summary>
public class ReservaFormViewModel
{
    public Reserva Reserva { get; init; } = new();
    public string DescricaoVeiculo { get; init; } = string.Empty;
    public string NomePessoa { get; init; } = string.Empty;
}
