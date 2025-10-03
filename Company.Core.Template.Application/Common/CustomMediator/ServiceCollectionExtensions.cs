using Company.Core.Template.Application.Common.CustomMediator;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static void AddRequestHandlersFromAssembly(
        this IServiceCollection services, 
        Assembly assembly)
    {
        // O tipo genérico aberto que estamos procurando (IRequestHandler<,>)
        var handlerInterfaceType = typeof(IRequestHandler<,>);

        // Encontra todos os tipos no assembly que implementam IRequestHandler<,>
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType));

        // Registra cada handler encontrado
        foreach (var handlerType in handlerTypes)
        {
            // Encontra a interface específica que o handler implementa 
            // (ex: IRequestHandler<GetProductByIdQuery, ProductResponse>)
            var implementedInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            // Registra o serviço com o contêiner de DI
            services.AddScoped(implementedInterface, handlerType);
        }
    }
}