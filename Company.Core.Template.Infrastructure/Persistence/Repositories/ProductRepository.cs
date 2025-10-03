using Company.Core.Template.Application.Common.Interfaces;
using Company.Core.Template.Domain.Entities;
using Company.Core.Template.Infrastructure.Telemetry;
using System.Diagnostics;

namespace Company.Core.Template.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private static readonly List<Product> _products = new();

    // IDs pré-definidos que podemos usar para testar
    public static readonly Guid LaptopId = new("8a2e771c-74a9-4a8a-a53a-9c7b3b3a0f1b");
    public static readonly Guid MouseId = new("2f5a6a68-0e9e-4c7b-9b1e-3e1b4b4e0f2b");
    public static readonly Guid TecladoId = new("b4e3f3e1-4c7b-4b1e-9b1e-3e1b4b4e0f2c");

    static ProductRepository()
    {
        // Usamos um truque com reflection para poder setar o ID na nossa entidade de domínio
        // que normalmente é protegido. Em um cenário real, o ORM faria isso.
        var laptop = Product.Create("Laptop Gamer", 7500.50m);
        SetProductId(laptop, LaptopId);

        var mouse = Product.Create("Mouse Vertical", 250.00m);
        SetProductId(mouse, MouseId);

        var teclado = Product.Create("Teclado Mecânico", 550.75m);
        SetProductId(teclado, TecladoId);

        _products.AddRange(new[] { laptop, mouse, teclado });
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        // Usando nossa fonte de instrumentação para criar um novo "span" (Activity)
        using var activity = Instrumentation.Source.StartActivity("repository.get_product_by_id", ActivityKind.Internal);

        // Adicionando "tags" ou "atributos" ao span com informações úteis
        activity?.SetTag("product.id", id);

        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is not null)
        {
            activity?.SetTag("product.found", true);
            Instrumentation.ProductsFoundCounter.Add(1,
            new KeyValuePair<string, object?>("product.id", id));
        }
        else
        {
            activity?.SetTag("product.found", false);
        }

        return Task.FromResult(product);
    }

    // Método auxiliar para setar o ID via reflection
    private static void SetProductId(Product product, Guid id)
    {
        var propertyInfo = typeof(Product).GetProperty(nameof(Product.Id));
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(product, id);
        }
    }
}