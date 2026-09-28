using Microsoft.AspNetCore.Mvc.Rendering;
using sistemaGaragem.Models;

namespace sistemaGaragem.ViewModels;

/// <summary>Tudo que a página inicial (Reservas) precisa exibir.</summary>
public class ReservaIndexViewModel
{
    public DateTime Hoje { get; init; }

    /// <summary>Dados do formulário "Nova reserva".</summary>
    public Reserva NovaReserva { get; init; } = new();

    public int? FiltroPessoaId { get; init; }

    public List<ReservaItemViewModel> Reservas { get; init; } = [];
    public List<VeiculoStatusViewModel> Veiculos { get; init; } = [];

    public List<SelectListItem> OpcoesVeiculos { get; init; } = [];
    public List<SelectListItem> OpcoesPessoas { get; init; } = [];
}
