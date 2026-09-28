using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using sistemaGaragem.Models;
using sistemaGaragem.Repositories;
using sistemaGaragem.ViewModels;

namespace sistemaGaragem.Controllers;

/// <summary>
/// Página inicial do sistema. Vincula um veículo a uma pessoa por um período.
/// A regra de conflito fica no IReservaRepository (ExisteConflito/ObterConflito);
/// o Controller só chama e exibe o resultado.
/// </summary>
public class ReservaController : Controller
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IPessoaRepository _pessoaRepository;

    public ReservaController(
        IReservaRepository reservaRepository,
        IVeiculoRepository veiculoRepository,
        IPessoaRepository pessoaRepository)
    {
        _reservaRepository = reservaRepository;
        _veiculoRepository = veiculoRepository;
        _pessoaRepository = pessoaRepository;
    }

    // GET /  e  GET /Reserva?pessoaId=2
    [HttpGet]
    public IActionResult Index(int? pessoaId)
    {
        var novaReserva = new Reserva { DataInicio = DateTime.Today, DataFim = DateTime.Today };
        return View(MontarPaginaInicial(novaReserva, pessoaId));
    }

    // POST /Reserva/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind(Prefix = nameof(ReservaIndexViewModel.NovaReserva))] Reserva reserva)
    {
        reserva.Id = 0;
        ValidarRegrasDaReserva(reserva, nameof(ReservaIndexViewModel.NovaReserva));

        if (!ModelState.IsValid)
            return View(nameof(Index), MontarPaginaInicial(reserva, null));

        _reservaRepository.Adicionar(reserva);
        TempData["Sucesso"] = "Reserva cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Reserva/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var reserva = _reservaRepository.ObterPorId(id);
        if (reserva is null)
            return NotFound();

        return View("ReservaForm", MontarFormulario(reserva));
    }

    // POST /Reserva/Edit/5  (só o período pode mudar)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind(Prefix = nameof(ReservaFormViewModel.Reserva))] Reserva dados)
    {
        var existente = _reservaRepository.ObterPorId(id);
        if (existente is null)
            return NotFound();

        // veículo e pessoa não mudam na edição: valem os que já estão gravados
        dados.Id = existente.Id;
        dados.VeiculoId = existente.VeiculoId;
        dados.PessoaId = existente.PessoaId;

        ValidarRegrasDaReserva(dados, nameof(ReservaFormViewModel.Reserva));

        if (!ModelState.IsValid)
            return View("ReservaForm", MontarFormulario(dados));

        _reservaRepository.Atualizar(dados);
        TempData["Sucesso"] = "Período da reserva atualizado.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Reserva/Delete/5  (cancelar reserva)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (_reservaRepository.ObterPorId(id) is null)
        {
            TempData["Erro"] = "Reserva não encontrada.";
        }
        else
        {
            _reservaRepository.Remover(id);
            TempData["Sucesso"] = "Reserva cancelada.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Pessoa e veículo precisam existir e o veículo não pode ter outra reserva no mesmo período.
    /// Os erros vão para o ModelState e aparecem no formulário — a aplicação não quebra.
    /// </summary>
    private void ValidarRegrasDaReserva(Reserva reserva, string prefixo)
    {
        if (reserva.PessoaId > 0 && _pessoaRepository.ObterPorId(reserva.PessoaId) is null)
            ModelState.AddModelError($"{prefixo}.{nameof(Reserva.PessoaId)}", "A pessoa selecionada não está cadastrada.");

        var veiculo = reserva.VeiculoId > 0 ? _veiculoRepository.ObterPorId(reserva.VeiculoId) : null;
        if (reserva.VeiculoId > 0 && veiculo is null)
            ModelState.AddModelError($"{prefixo}.{nameof(Reserva.VeiculoId)}", "O veículo selecionado não está cadastrado.");

        if (!ModelState.IsValid || veiculo is null)
            return;

        var conflito = _reservaRepository.ObterConflito(
            reserva.VeiculoId, reserva.DataInicio, reserva.DataFim,
            idIgnorado: reserva.Id == 0 ? null : reserva.Id);

        if (conflito is not null)
        {
            ModelState.AddModelError(string.Empty,
                $"Reserva bloqueada: o veículo {veiculo.Placa} já está reservado de " +
                $"{conflito.DataInicio:dd/MM/yyyy} a {conflito.DataFim:dd/MM/yyyy}, " +
                "e esse período se sobrepõe ao informado.");
        }
    }

    private ReservaIndexViewModel MontarPaginaInicial(Reserva novaReserva, int? pessoaId)
    {
        var hoje = DateTime.Today;
        var pessoas = _pessoaRepository.ObterTodas();
        var veiculos = _veiculoRepository.ObterTodos();
        var reservas = _reservaRepository.ObterTodas();

        if (pessoaId is > 0)
            reservas = reservas.Where(r => r.PessoaId == pessoaId).ToList();

        var pessoasPorId = pessoas.ToDictionary(p => p.Id);
        var veiculosPorId = veiculos.ToDictionary(v => v.Id);

        return new ReservaIndexViewModel
        {
            Hoje = hoje,
            NovaReserva = novaReserva,
            FiltroPessoaId = pessoaId,

            Reservas = reservas.Select(r =>
            {
                veiculosPorId.TryGetValue(r.VeiculoId, out var veiculo);
                pessoasPorId.TryGetValue(r.PessoaId, out var pessoa);

                return new ReservaItemViewModel
                {
                    Id = r.Id,
                    Placa = veiculo?.Placa ?? "(veículo removido)",
                    Modelo = veiculo is null ? "—" : $"{veiculo.Marca} {veiculo.Modelo}",
                    NomePessoa = pessoa?.Nome ?? "(pessoa removida)",
                    DataInicio = r.DataInicio,
                    DataFim = r.DataFim,
                    QuantidadeDias = r.QuantidadeDias,
                    Situacao = r.SituacaoEm(hoje)
                };
            }).ToList(),

            // Status calculado a partir das reservas
            Veiculos = veiculos.Select(veiculo => new VeiculoStatusViewModel
            {
                Veiculo = veiculo,
                Disponivel = !_reservaRepository.VeiculoReservadoEm(veiculo.Id, hoje)
            }).ToList(),

            OpcoesVeiculos = veiculos
                .Select(veiculo => new SelectListItem(veiculo.Descricao, veiculo.Id.ToString()))
                .ToList(),

            OpcoesPessoas = pessoas
                .Select(pessoa => new SelectListItem($"{pessoa.Nome} ({pessoa.Cpf})", pessoa.Id.ToString()))
                .ToList()
        };
    }

    private ReservaFormViewModel MontarFormulario(Reserva reserva)
    {
        var veiculo = _veiculoRepository.ObterPorId(reserva.VeiculoId);
        var pessoa = _pessoaRepository.ObterPorId(reserva.PessoaId);

        return new ReservaFormViewModel
        {
            Reserva = reserva,
            DescricaoVeiculo = veiculo?.Descricao ?? "(veículo removido)",
            NomePessoa = pessoa?.Nome ?? "(pessoa removida)"
        };
    }
}
