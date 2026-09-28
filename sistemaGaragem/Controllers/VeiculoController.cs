using Microsoft.AspNetCore.Mvc;
using sistemaGaragem.Models;
using sistemaGaragem.Repositories;
using sistemaGaragem.ViewModels;

namespace sistemaGaragem.Controllers;

/// <summary>
/// CRUD de veículos, na mesma arquitetura de Pessoa:
/// dependências chegam pelo construtor como interfaces.
/// </summary>
public class VeiculoController : Controller
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IReservaRepository _reservaRepository;

    public VeiculoController(IVeiculoRepository veiculoRepository, IReservaRepository reservaRepository)
    {
        _veiculoRepository = veiculoRepository;
        _reservaRepository = reservaRepository;
    }

    // GET /Veiculo
    public IActionResult Index()
    {
        var hoje = DateTime.Today;
        var modelo = _veiculoRepository.ObterTodos()
            .Select(v => new VeiculoStatusViewModel
            {
                Veiculo = v,
                Disponivel = !_reservaRepository.VeiculoReservadoEm(v.Id, hoje)
            })
            .ToList();

        return View(modelo);
    }

    // GET /Veiculo/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View("VeiculoForm", new Veiculo { Ano = DateTime.Today.Year });
    }

    // POST /Veiculo/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Veiculo veiculo)
    {
        veiculo.Id = 0;
        ValidarPlacaUnica(veiculo);

        if (!ModelState.IsValid)
            return View("VeiculoForm", veiculo);

        _veiculoRepository.Adicionar(veiculo);
        TempData["Sucesso"] = $"Veículo {veiculo.Placa} cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Veiculo/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var veiculo = _veiculoRepository.ObterPorId(id);
        if (veiculo is null)
            return NotFound();

        return View("VeiculoForm", veiculo);
    }

    // POST /Veiculo/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Veiculo veiculo)
    {
        if (id != veiculo.Id)
            return BadRequest();

        if (_veiculoRepository.ObterPorId(id) is null)
            return NotFound();

        ValidarPlacaUnica(veiculo);

        if (!ModelState.IsValid)
            return View("VeiculoForm", veiculo);

        _veiculoRepository.Atualizar(veiculo);
        TempData["Sucesso"] = $"Veículo {veiculo.Placa} atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Veiculo/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var veiculo = _veiculoRepository.ObterPorId(id);

        if (veiculo is null)
        {
            TempData["Erro"] = "Veículo não encontrado.";
        }
        else if (_reservaRepository.VeiculoPossuiReservaVigente(id, DateTime.Today))
        {
            TempData["Erro"] = $"Não é possível excluir o veículo {veiculo.Placa}: ele possui reserva em andamento ou futura.";
        }
        else
        {
            _veiculoRepository.Remover(id);
            TempData["Sucesso"] = $"Veículo {veiculo.Placa} excluído.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidarPlacaUnica(Veiculo veiculo)
    {
        if (!string.IsNullOrWhiteSpace(veiculo.Placa) &&
            _veiculoRepository.PlacaJaCadastrada(veiculo.Placa, veiculo.Id))
        {
            ModelState.AddModelError(nameof(Veiculo.Placa), "Já existe um veículo cadastrado com esta placa.");
        }
    }
}
