using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Company.Core.Template.Api.Middleware;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Usaremos ProblemDetails para um formato de erro padronizado (RFC 7807)
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "An internal server error has occurred."
            // Você pode adicionar mais detalhes aqui, se necessário, ou customizar
            // a resposta baseada no tipo da exceção.
        };

        // TODO: Em um ambiente de desenvolvimento, você pode querer adicionar mais detalhes
        // if (environment.IsDevelopment())
        // {
        //     problemDetails.Detail = exception.ToString();
        // }

        var jsonResponse = JsonSerializer.Serialize(problemDetails);
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(jsonResponse);
    }
}