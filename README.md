# Garagem de Veículos

Sistema de cadastro de **Pessoas**, **Veículos** e **Reservas** (vínculo de um veículo a uma pessoa por um período), feito para a Atividade Prática da disciplina **Arquitetura de Software (ESW430) — UniRV**.

## Integrantes

- Nome do integrante 1
- Nome do integrante 2
- Nome do integrante 3
- Nome do integrante 4

## Tecnologias

- **Linguagem:** C#
- **Framework:** ASP.NET Core MVC (.NET 10)
- **Persistência:** arquivos `.json` com `System.Text.Json`
- **Interface:** Razor Views + Bootstrap 5 (via CDN)

## Como executar

Pré-requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
# na pasta raiz do repositório (onde está o sistemaGaragem.slnx)
dotnet run --project sistemaGaragem
```

Abra **http://localhost:5080**. A página inicial é a de **Reservas**.

No Visual Studio 2022/2026 basta abrir `sistemaGaragem.slnx` e apertar F5.

Os dados ficam em `sistemaGaragem/Data/` (`pessoas.json`, `veiculos.json`, `reservas.json`) e continuam lá depois de fechar e abrir a aplicação. Se algum arquivo for apagado, o repositório correspondente o recria com `[]`.

## Arquitetura (MVC + Repository + Injeção de Dependência)

Caminho dos dados: **View → Controller → Interface → Repositório → arquivo JSON**.

```
sistemaGaragem/
├── Controllers/        PessoaController, VeiculoController, ReservaController
├── Models/             Pessoa, Veiculo, Reserva  (+ Validacoes/CpfAttribute, AnoValidoAttribute)
├── Repositories/       IPessoaRepository / PessoaRepository
│                       IVeiculoRepository / VeiculoRepository
│                       IReservaRepository / ReservaRepository
├── ViewModels/         dados montados para as telas de Reserva e Veículo
├── Views/
│   ├── Pessoa/         Index.cshtml, PessoaForm.cshtml
│   ├── Veiculo/        Index.cshtml, VeiculoForm.cshtml
│   └── Reserva/        Index.cshtml, ReservaForm.cshtml
├── Data/               pessoas.json, veiculos.json, reservas.json
└── Program.cs          registro da DI e rota padrão (Reserva/Index)
```

Registro da injeção de dependência em `Program.cs`:

```csharp
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
```

Os Controllers recebem **interfaces** pelo construtor e nunca leem/gravam arquivo, nunca fazem `new ...Repository()` e não guardam listas `static`.

### SOLID no código

| Princípio | Onde aparece |
| --- | --- |
| **S** — Responsabilidade Única | `Models` guardam dados e validações (DataAnnotations, `CpfAttribute`, regra de sobreposição em `Reserva.SobrepoeA`). Cada `Repository` só lê/grava o seu JSON. Controllers só recebem a requisição, chamam o repositório e escolhem a View. |
| **O** — Aberto/Fechado | Para trocar JSON por banco (EF Core), basta criar, por exemplo, `PessoaEfRepository : IPessoaRepository` e mudar uma linha no `Program.cs`. Controllers e Views não mudam. |
| **L** — Substituição de Liskov | Qualquer implementação de `IPessoaRepository` (JSON, memória, banco, mock de teste) funciona no `PessoaController` sem ajuste. |
| **I** — Segregação de Interfaces | Uma interface por entidade, cada uma só com os métodos que aquela entidade usa (`CpfJaCadastrado` só em Pessoa, `PlacaJaCadastrada` só em Veículo, `ExisteConflito` só em Reserva). |
| **D** — Inversão de Dependência | Controllers dependem das abstrações (`I...Repository`), recebidas pelo construtor. O container de DI do ASP.NET Core entrega as implementações concretas. |

## Funcionalidades

**Pessoas** (`/Pessoa`) — listar, cadastrar, editar e excluir (com confirmação). Nome, CPF e e-mail obrigatórios; CPF validado pelos dígitos verificadores e único; e-mail com formato válido; telefone opcional.

**Veículos** (`/Veiculo`) — CRUD completo. Placa obrigatória e única (padrão antigo `ABC-1234` ou Mercosul `ABC1D23`); marca, modelo e ano obrigatórios; cor opcional. A listagem mostra se o veículo está Disponível ou Reservado hoje.

**Reservas** (página inicial `/`)

- Lista as reservas com veículo (placa e modelo), pessoa e período, com filtro por pessoa.
- Cria reserva escolhendo veículo e pessoa em selects carregados dos repositórios, mais data de início e fim.
- Edita o período e cancela reservas.
- Mostra cada veículo como **Disponível** ou **Reservado** na data de hoje (calculado a partir das reservas).

Regra de negócio: um veículo não pode ter duas reservas com períodos sobrepostos. Há conflito quando
`novaInicio <= existenteFim E novaFim >= existenteInicio`. A verificação está em `IReservaRepository.ExisteConflito(veiculoId, inicio, fim, idIgnorado)` / `ObterConflito(...)`; o Controller só chama e mostra a mensagem. Na edição o próprio Id é ignorado. Também são validados: data de fim não anterior à de início, e pessoa/veículo existentes.

Os dados de exemplo já trazem a reserva **10/10 a 15/10** do veículo `BRA2E19`, então dá para testar a tabela do enunciado:

| Nova reserva (BRA2E19) | Resultado |
| --- | --- |
| 16/10 a 20/10 | Permitida |
| 14/10 a 18/10 | Bloqueada |
| 15/10 a 17/10 | Bloqueada |
| 01/10 a 31/10 | Bloqueada |

**Desafio extra implementado:** não é possível excluir pessoa ou veículo que tenha reserva em andamento ou futura. Filtro de reservas por pessoa.

## Prints

Ver pasta [`docs/`](docs/).

## Ferramentas de IA utilizadas

- Claude (Anthropic) — apoio na geração do código e do README.
