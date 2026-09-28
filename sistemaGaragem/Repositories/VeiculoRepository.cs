using System.Text.Encodings.Web;
using System.Text.Json;
using sistemaGaragem.Models;

namespace sistemaGaragem.Repositories;

/// <summary>
/// Implementação de IVeiculoRepository com persistência em Data/veiculos.json.
/// É a ÚNICA classe do sistema que lê ou grava esse arquivo.
/// </summary>
public class VeiculoRepository : IVeiculoRepository
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

    public VeiculoRepository(IWebHostEnvironment ambiente)
    {
        _caminhoArquivo = Path.Combine(ambiente.ContentRootPath, "Data", "veiculos.json");
    }

    public List<Veiculo> ObterTodos()
    {
        lock (Trava)
        {
            return Ler().OrderBy(v => v.Placa).ToList();
        }
    }

    public Veiculo? ObterPorId(int id)
    {
        lock (Trava)
        {
            return Ler().FirstOrDefault(v => v.Id == id);
        }
    }

    public void Adicionar(Veiculo veiculo)
    {
        lock (Trava)
        {
            var veiculos = Ler();
            veiculo.Id = veiculos.Count == 0 ? 1 : veiculos.Max(v => v.Id) + 1;
            veiculos.Add(veiculo);
            Gravar(veiculos);
        }
    }

    public void Atualizar(Veiculo veiculo)
    {
        lock (Trava)
        {
            var veiculos = Ler();
            var indice = veiculos.FindIndex(v => v.Id == veiculo.Id);
            if (indice < 0)
                throw new KeyNotFoundException($"Veículo {veiculo.Id} não encontrado.");

            veiculos[indice] = veiculo;
            Gravar(veiculos);
        }
    }

    public void Remover(int id)
    {
        lock (Trava)
        {
            var veiculos = Ler();
            if (veiculos.RemoveAll(v => v.Id == id) > 0)
                Gravar(veiculos);
        }
    }

    public bool PlacaJaCadastrada(string placa, int idIgnorado = 0)
    {
        var normalizada = new Veiculo { Placa = placa }.PlacaSemHifen; // mesma normalização do Model
        lock (Trava)
        {
            return Ler().Any(v => v.Id != idIgnorado && v.PlacaSemHifen == normalizada);
        }
    }

    // ---------- acesso ao arquivo ----------

    private List<Veiculo> Ler()
    {
        if (!File.Exists(_caminhoArquivo))
        {
            Gravar([]);
            return [];
        }

        var json = File.ReadAllText(_caminhoArquivo);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<Veiculo>>(json, OpcoesJson) ?? [];
    }

    private void Gravar(List<Veiculo> veiculos)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_caminhoArquivo)!);
        File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(veiculos, OpcoesJson));
    }
}
