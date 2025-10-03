namespace Company.Core.Template.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    // Construtor para ser usado pelo ORM ou para recriar o objeto
    private Product(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    // Método de fábrica estático para criar um novo produto, garantindo as regras de negócio
    public static Product Create(string name, decimal price)
    {
        // Aqui você poderia adicionar validações de domínio
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        }
        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative.", nameof(price));
        }

        return new Product(Guid.NewGuid(), name, price);
    }
}