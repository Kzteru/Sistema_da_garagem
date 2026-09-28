using sistemaGaragem.Models;

namespace sistemaGaragem.Repositories;

/// <summary>
/// Contrato do repositório de reservas.
/// A verificação de conflito de período fica aqui, fora do Controller.
/// </summary>
public interface IReservaRepository
{
    List<Reserva> ObterTodas();
    Reserva? ObterPorId(int id);
    void Adicionar(Reserva reserva);
    void Atualizar(Reserva reserva);
    void Remover(int id);

    /// <summary>
    /// True se o veículo já tem outra reserva cujo período se sobrepõe a [inicio, fim].
    /// <paramref name="idIgnorado"/> serve para a edição: a reserva não conflita com ela mesma.
    /// </summary>
    bool ExisteConflito(int veiculoId, DateTime inicio, DateTime fim, int? idIgnorado = null);

    /// <summary>Igual a ExisteConflito, mas devolve a reserva conflitante (para mostrar na mensagem).</summary>
    Reserva? ObterConflito(int veiculoId, DateTime inicio, DateTime fim, int? idIgnorado = null);

    /// <summary>True se o veículo está reservado no dia informado (status Disponível/Reservado).</summary>
    bool VeiculoReservadoEm(int veiculoId, DateTime dia);

    /// <summary>True se a pessoa tem reserva em andamento ou futura.</summary>
    bool PessoaPossuiReservaVigente(int pessoaId, DateTime hoje);

    /// <summary>True se o veículo tem reserva em andamento ou futura.</summary>
    bool VeiculoPossuiReservaVigente(int veiculoId, DateTime hoje);
}
