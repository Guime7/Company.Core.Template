using System.Diagnostics;
using System.Diagnostics.Metrics; // Adicione este using

namespace Company.Core.Template.Infrastructure.Telemetry;

public static class Instrumentation
{
    public static readonly ActivitySource Source = new("Company.Core.Template.Api");

    // 1. Crie um "Meter"
    private static readonly Meter Meter = new("Company.Core.Template.Api");

    // 2. Crie um "Counter" (contador)
    public static readonly Counter<int> ProductsFoundCounter =
        Meter.CreateCounter<int>("products.found.count", description: "Contagem de produtos encontrados com sucesso.");
}