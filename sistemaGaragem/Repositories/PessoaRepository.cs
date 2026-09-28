using System.Text.Encodings.Web;
using System.Text.Json;
using sistemaGaragem.Models;
using sistemaGaragem.Models.Validacoes;

namespace sistemaGaragem.Repositories;

/// <summary>
/// Implementação de IPessoaRepository com persistência em Data/pessoas.json.
/// É a ÚNICA classe do sistema que lê ou grava esse arquivo.
/// </summary>
public class PessoaRepository : IPessoaRepository
{
    // Trava compartilhada entre as instâncias (Scoped = uma por requisição),
    // para duas requisições não gravarem o arquivo ao mesmo tempo.
    private static readonly Lock Trava = new();

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // mantém acentos legíveis no arquivo
    };

    private readonly string _caminhoArquivo;

    public PessoaRepository(IWebHostEnvironment ambiente)
    {
        _caminhoArquivo = Path.Combine(ambiente.ContentRootPath, "Data", "pessoas.json");
    }

    public List<Pessoa> ObterTodas()
    {
        lock (Trava)
        {
            return Ler().OrderBy(p => p.Nome).ToList();
        }
    }

    public Pessoa? ObterPorId(int id)
    {
        lock (Trava)
        {
            return Ler().FirstOrDefault(p => p.Id == id);
        }
    }

    public void Adicionar(Pessoa pessoa)
    {
        lock (Trava)
        {
            var pessoas = Ler();
            pessoa.Id = pessoas.Count == 0 ? 1 : pessoas.Max(p => p.Id) + 1; // maior Id + 1
            pessoas.Add(pessoa);
            Gravar(pessoas);
        }
    }

    public void Atualizar(Pessoa pessoa)
    {
        lock (Trava)
        {
            var pessoas = Ler();
            var indice = pessoas.FindIndex(p => p.Id == pessoa.Id);
            if (indice < 0)
                throw new KeyNotFoundException($"Pessoa {pessoa.Id} não encontrada.");

            pessoas[indice] = pessoa;
            Gravar(pessoas);
        }
    }

    public void Remover(int id)
    {
        lock (Trava)
        {
            var pessoas = Ler();
            if (pessoas.RemoveAll(p => p.Id == id) > 0)
                Gravar(pessoas);
        }
    }

    public bool CpfJaCadastrado(string cpf, int idIgnorado = 0)
    {
        var digitos = CpfAttribute.SomenteDigitos(cpf);
        lock (Trava)
        {
            return Ler().Any(p => p.Id != idIgnorado && p.CpfSomenteDigitos == digitos);
        }
    }

    // ---------- acesso ao arquivo ----------

    private List<Pessoa> Ler()
    {
        if (!File.Exists(_caminhoArquivo))
        {
            Gravar([]); // arquivo inexistente é criado com []
            return [];
        }

        var json = File.ReadAllText(_caminhoArquivo);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<Pessoa>>(json, OpcoesJson) ?? [];
    }

    private void Gravar(List<Pessoa> pessoas)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_caminhoArquivo)!);
        File.WriteAllText(_caminhoArquivo, JsonSerializer.Serialize(pessoas, OpcoesJson));
    }
}
