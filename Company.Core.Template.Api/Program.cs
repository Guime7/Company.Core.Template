using Company.Core.Template.Api.Middleware;
using Company.Core.Template.Application.Common.Behaviors;
using Company.Core.Template.Application.Common.CustomMediator;
using Company.Core.Template.Application.Common.Interfaces; // Adicionar using
using Company.Core.Template.Application.Features.Products;
using Company.Core.Template.Application.UseCases.Queries;
using Company.Core.Template.Infrastructure.Persistence.Repositories; // Adicionar using
using Company.Core.Template.Infrastructure.Telemetry;
//using MediatR;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection; // Adicionar using

var builder = WebApplication.CreateBuilder(args);
var appAssembly = typeof(Company.Core.Template.Application.Common.CustomMediator.IMediator).Assembly;

builder.Services.Configure<OpenTelemetryLoggerOptions>(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});

// Adiciona configura��o para incluir TraceId e SpanId nos logs do console
builder.Logging.ClearProviders();

// **IN�CIO DA CONFIGURA��O OPENTELEMETRY**

// 1. Defina um nome para o servi�o/recurso. Essencial para identificar sua aplica��o.
var serviceName = "Company.Core.Template.Api";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddSource(serviceName) // Permite criar traces customizados (veremos a seguir)
        .AddAspNetCoreInstrumentation() // Instrumenta��o autom�tica do ASP.NET Core
        .AddHttpClientInstrumentation()   // Instrumenta��o autom�tica de chamadas HTTP
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
            // Tamb�m envia logs para o receiver OTLP
            opt.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        //.AddRuntimeInstrumentation() // Coleta m�tricas do runtime do .NET (GC, JIT, etc.)
        .AddOtlpExporter(opt =>
        {
            // Tamb�m envia m�tricas para o receiver OTLP
            opt.Endpoint = new Uri("http://localhost:4317");
        }));

// **FIM DA CONFIGURA��O OPENTELEMETRY**

// Adicionar servi�os ao container.

//// 1. Registrar MediatR para encontrar os handlers no projeto Application
//builder.Services.AddMediatR(cfg =>
//    cfg.RegisterServicesFromAssembly(typeof(Company.Core.Template.Application.Common.Interfaces.IProductRepository).Assembly));

//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));

// **REGISTRO DO NOSSO MEDIADOR CUSTOMIZADO**

// 1. Registra a implementa��o do Mediator
builder.Services.AddScoped<IMediator, Mediator>();

//builder.Services.AddScoped<IRequestHandler<GetProductByIdQuery, ProductResponse>, GetProductByIdQueryHandler>();

// 2. Escaneia o assembly da Application e registra todos os IRequestHandlers
builder.Services.Scan(selector => selector
    .FromAssemblies(appAssembly) // 1. Look in this assembly
    .AddClasses(filter => filter.AssignableTo(typeof(IRequestHandler<,>))) // 2. Find classes that look like a handler
    .AsImplementedInterfaces() // 3. Register them by the interfaces they implement
    .WithScopedLifetime()); // 4. Use a scoped lifetime for them

// 3. Registra os behaviors (exemplo com nosso TracingBehavior adaptado)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));



// 2. Registrar o Reposit�rio (usando Singleton para manter a lista em mem�ria)
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