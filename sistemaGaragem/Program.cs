using System.Globalization;
using Microsoft.AspNetCore.Localization;
using sistemaGaragem.Repositories;

var builder = WebApplication.CreateBuilder(args);

// MVC + mensagens de erro de binding em português
builder.Services.AddControllersWithViews(options =>
{
    var mensagens = options.ModelBindingMessageProvider;
    mensagens.SetValueMustNotBeNullAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
    mensagens.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado para {campo} é inválido.");
});

// Injeção de Dependência: os Controllers pedem a INTERFACE,
// o container entrega a implementação concreta (JSON).
// Para trocar JSON por banco de dados, basta mudar estas linhas.
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new List<CultureInfo> { cultura },
    SupportedUICultures = new List<CultureInfo> { cultura }
});

app.UseRouting();

app.MapStaticAssets();

// Página inicial = Reservas
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Reserva}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
