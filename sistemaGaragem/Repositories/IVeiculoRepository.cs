using sistemaGaragem.Models;

namespace sistemaGaragem.Repositories;

/// <summary>Contrato do repositório de veículos.</summary>
public interface IVeiculoRepository
{
    List<Veiculo> ObterTodos();
    Veiculo? ObterPorId(int id);
    void Adicionar(Veiculo veiculo);
    void Atualizar(Veiculo veiculo);
    void Remover(int id);

    /// <summary>True se outro veículo (Id diferente de <paramref name="idIgnorado"/>) já usa esta placa.</summary>
    bool PlacaJaCadastrada(string placa, int idIgnorado = 0);
}
