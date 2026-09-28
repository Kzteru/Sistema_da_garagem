using sistemaGaragem.Models;

namespace sistemaGaragem.Repositories;

/// <summary>
/// Contrato do repositório de pessoas. O Controller conhece SÓ esta interface,
/// nunca a classe concreta nem o arquivo JSON.
/// </summary>
public interface IPessoaRepository
{
    List<Pessoa> ObterTodas();
    Pessoa? ObterPorId(int id);
    void Adicionar(Pessoa pessoa);
    void Atualizar(Pessoa pessoa);
    void Remover(int id);

    /// <summary>True se outra pessoa (Id diferente de <paramref name="idIgnorado"/>) já usa este CPF.</summary>
    bool CpfJaCadastrado(string cpf, int idIgnorado = 0);
}
