using System.Text.Encodings.Web;
using System.Text.Json;
using sistemaGaragem.Models;

namespace sistemaGaragem.Repositories;

/// <summary>
/// Implementação de IReservaRepository com persistência em Data/reservas.json.
/// É a ÚNICA classe do sistema que lê ou grava esse arquivo.
/// </summary>
public class ReservaRepository : IReservaRepository
{
    private static readonly Lock Trava = new();

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _caminhoArquivo;

    public ReservaRepository(IWebHostEnvironment ambiente)
    {
        _caminhoArquivo = Path.Combine(ambiente.ContentRootPath, "Data", "reservas.json");
    }

    public List<Reserva> ObterTodas()
    {
        lock (Trava)
        {
            return Ler().OrderByDescending(r => r.DataInicio).ToList();
        }
    }

    public Reserva? ObterPorId(int id)
    {
        lock (Trava)
        {
            return Ler().FirstOrDefault(r => r.Id == id);
        }
    }

    public void Adicionar(Reserva reserva)
    {
        lock (Trava)
        {
            var reservas = Ler();
            reserva.Id = reservas.Count == 0 ? 1 : reservas.Max(r => r.Id) + 1;
            reservas.Add(reserva);
            Gravar(reservas);
        }
    }

    public void Atualizar(Reserva reserva)
    {
        lock (Trava)
        {
            var reservas = Ler();
            var indice = reservas.FindIndex(r => r.Id == reserva.Id);
            if (indice < 0)
                throw new KeyNotFoundException($"Reserva {reserva.Id} não encontrada.");

            reservas[indice] = reserva;
            Gravar(reservas);
        }
    }

    public void Remover(int id)
    {
        lock (Trava)
        {
            var reservas = Ler();
            if (reservas.RemoveAll(r => r.Id == id) > 0)
                Gravar(reservas);
        }
    }

    public bool ExisteConflito(int veiculoId, DateTime inicio, DateTime fim, int? idIgnorado = null) =>
        ObterConflito(veiculoId, inicio, fim, idIgnorado) is not null;

    public Reserva? ObterConflito(int veiculoId, DateTime inicio, DateTime fim, int? idIgnorado = null)
    {
        lock (Trava)
        {
            // Conflito: mesmo veículo E novaInicio <= existenteFim E novaFim >= existenteInicio
            return Ler().FirstOrDefault(r =>
                r.VeiculoId == veiculoId &&
                r.Id != idIgnorado &&
                r.SobrepoeA(inicio, fim));
        }
    }

    public bool VeiculoReservadoEm(int veiculoId, DateTime dia) =>
        ExisteConflito(veiculoId, dia, dia);

    public bool PessoaPossuiReservaVigente(int pessoaId, DateTime hoje)
    {
        lock (Trava)
        {
            return Ler().Any(r => r.PessoaId == pessoaId && r.DataFim >= hoje.Date);
        }
    }

    public bool VeiculoPossuiReservaVigente(int veiculoId, DateTime hoje)
    {
        lock (Trava)
        {
            return Ler().Any(r => r.VeiculoId == veiculoId && r.DataFim >= hoje.Date);
        }
    }

    // ---------- acesso ao arquivo ----------

    private List<Reserva> Ler()
    {
        if (!File.Exists(_caminhoArquivo))
        {
            Gravar([]);
            return [];
        }

        var json = File.ReadAllText(_caminhoArquivo);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<Reserva>>(json, OpcoesJson) ?? [];
    }

    private void Gravar(List<Reserva> reservas)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_caminhoArquivo)!);
        File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(reservas, OpcoesJson));
    }
}
