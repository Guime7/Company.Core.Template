using Company.Core.Template.Domain.Entities;

namespace Company.Core.Template.Application.Common.Interfaces;

public interface IProductRepository
{
    // O '?' indica que o produto pode não ser encontrado e o retorno ser nulo.
    Task<Product?> GetByIdAsync(Guid id);

    // Podemos adicionar outros métodos no futuro
    // Task AddAsync(Product product);
}