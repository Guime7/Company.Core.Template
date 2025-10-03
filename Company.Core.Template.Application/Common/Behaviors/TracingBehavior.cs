using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Company.Core.Template.Application.Common.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Usaremos uma ActivitySource estática, mas poderia ser injetada.
    // O nome aqui deve ser o mesmo que você registrou no Program.cs
    private static readonly ActivitySource ActivitySource = new("Company.Core.Template.Api");
    private readonly ILogger<TracingBehavior<TRequest, TResponse>> _logger;

    public TracingBehavior(ILogger<TracingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Pega o nome da requisição (ex: "GetProductByIdQuery") para usar como nome do Span
        var requestName = typeof(TRequest).Name;

        // Inicia o Activity/Span
        using var activity = ActivitySource.StartActivity($"MediatR Request: {requestName}", ActivityKind.Internal);

        _logger.LogInformation("Iniciando requisição MediatR: {RequestName}", requestName);

        try
        {
            // Adiciona tags úteis ao span para facilitar a busca e análise no Jaeger
            activity?.SetTag("mediatr.request.name", requestName);
            // Poderíamos adicionar os dados da requisição, mas cuidado com dados sensíveis
            // activity?.SetTag("mediatr.request.data", JsonSerializer.Serialize(request));

            // Chama o próximo behavior na cadeia ou o handler final
            var response = await next();

            _logger.LogInformation("Finalizada requisição MediatR: {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            // Se ocorrer um erro, gravamos a exceção no span para que ela apareça no Jaeger
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            //activity?.RecordException(ex);

            _logger.LogError(ex, "Erro na requisição MediatR: {RequestName}", requestName);

            throw; // Re-lança a exceção para não alterar o comportamento da aplicação
        }
    }
}