using Company.Core.Template.Api.Middleware;
using Company.Core.Template.Application.Common.Behaviors;
using Company.Core.Template.Application.Common.Interfaces; // Adicionar using
using Company.Core.Template.Infrastructure.Persistence.Repositories; // Adicionar using
using Company.Core.Template.Infrastructure.Telemetry;
using MediatR;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection; // Adicionar using

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<OpenTelemetryLoggerOptions>(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

// Adiciona configuração para incluir TraceId e SpanId nos logs do console
builder.Logging.ClearProviders();

// **INÍCIO DA CONFIGURAÇÃO OPENTELEMETRY**

// 1. Defina um nome para o serviço/recurso. Essencial para identificar sua aplicação.
var serviceName = "Company.Core.Template.Api";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddSource(serviceName) // Permite criar traces customizados (veremos a seguir)
        .AddAspNetCoreInstrumentation() // Instrumentação automática do ASP.NET Core
        .AddHttpClientInstrumentation()   // Instrumentação automática de chamadas HTTP
        .AddOtlpExporter(opt =>
        {
            // Envia os traces para o Jaeger que rodamos no Docker
            opt.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithLogging(logging => logging
        .AddConsoleExporter() // Exporta logs para o console
        .AddProcessor(new ActivityEventLogProcessor()) // Adiciona o processor customizado
        .AddOtlpExporter(opt =>
        {
            // Também envia logs para o receiver OTLP
            opt.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        //.AddRuntimeInstrumentation() // Coleta métricas do runtime do .NET (GC, JIT, etc.)
        .AddOtlpExporter(opt =>
        {
            // Também envia métricas para o receiver OTLP
            opt.Endpoint = new Uri("http://localhost:4317");
        }));

// **FIM DA CONFIGURAÇÃO OPENTELEMETRY**

// Adicionar serviços ao container.

// 1. Registrar MediatR para encontrar os handlers no projeto Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Company.Core.Template.Application.Common.Interfaces.IProductRepository).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));


// 2. Registrar o Repositório (usando Singleton para manter a lista em memória)
builder.Services.AddSingleton<IProductRepository, ProductRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();