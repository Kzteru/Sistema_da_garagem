using Microsoft.AspNetCore.Mvc;
using sistemaGaragem.Models;
using sistemaGaragem.Repositories;

namespace sistemaGaragem.Controllers;

/// <summary>
/// Recebe as requisições de Pessoa, chama o repositório e escolhe a View.
/// Não sabe COMO os dados são salvos: só conhece as interfaces, recebidas pelo construtor.
/// </summary>
public class PessoaController : Controller
{
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IReservaRepository _reservaRepository;

    public PessoaController(IPessoaRepository pessoaRepository, IReservaRepository reservaRepository)
    {
        _pessoaRepository = pessoaRepository;
        _reservaRepository = reservaRepository;
    }

    // GET /Pessoa
    public IActionResult Index()
    {
        return View(_pessoaRepository.ObterTodas());
    }

    // GET /Pessoa/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View("PessoaForm", new Pessoa());
    }

    // POST /Pessoa/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Pessoa pessoa)
    {
        pessoa.Id = 0;
        ValidarCpfUnico(pessoa);

        if (!ModelState.IsValid)
            return View("PessoaForm", pessoa);

        _pessoaRepository.Adicionar(pessoa);
        TempData["Sucesso"] = $"Pessoa \"{pessoa.Nome}\" cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Pessoa/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var pessoa = _pessoaRepository.ObterPorId(id);
        if (pessoa is null)
            return NotFound();

        return View("PessoaForm", pessoa);
    }

    // POST /Pessoa/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Pessoa pessoa)
    {
        if (id != pessoa.Id)
            return BadRequest();

        if (_pessoaRepository.ObterPorId(id) is null)
            return NotFound();

        ValidarCpfUnico(pessoa);

        if (!ModelState.IsValid)
            return View("PessoaForm", pessoa);

        _pessoaRepository.Atualizar(pessoa);
        TempData["Sucesso"] = $"Pessoa \"{pessoa.Nome}\" atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Pessoa/Delete/5  (a confirmação é feita na tela antes do envio)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var pessoa = _pessoaRepository.ObterPorId(id);

        if (pessoa is null)
        {
            TempData["Erro"] = "Pessoa não encontrada.";
        }
        else if (_reservaRepository.PessoaPossuiReservaVigente(id, DateTime.Today))
        {
            TempData["Erro"] = $"Não é possível excluir \"{pessoa.Nome}\": ela possui reserva em andamento ou futura.";
        }
        else
        {
            _pessoaRepository.Remover(id);
            TempData["Sucesso"] = $"Pessoa \"{pessoa.Nome}\" excluída.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidarCpfUnico(Pessoa pessoa)
    {
        if (!string.IsNullOrWhiteSpace(pessoa.Cpf) &&
            _pessoaRepository.CpfJaCadastrado(pessoa.Cpf, pessoa.Id))
        {
            ModelState.AddModelError(nameof(Pessoa.Cpf), "Já existe uma pessoa cadastrada com este CPF.");
        }
    }
}
