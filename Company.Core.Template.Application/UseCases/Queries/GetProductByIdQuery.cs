using Company.Core.Template.Application.Common.Interfaces;
using MediatR; // Adicione o pacote NuGet MediatR a este projeto

namespace Company.Core.Template.Application.Features.Products;

// 1. A Query: Os dados de entrada para o caso de uso. Usamos 'record' pela imutabilidade.
public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse>;

// 2. A Resposta (DTO): O objeto que será retornado para a camada de apresentação.
// Evita vazar a entidade de domínio para fora da camada de aplicação.
public sealed record ProductResponse(Guid Id, string Name, decimal Price);

