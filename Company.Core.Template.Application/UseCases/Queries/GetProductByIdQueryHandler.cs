using Company.Core.Template.Application.Common.Interfaces;
using Company.Core.Template.Application.Features.Products;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Company.Core.Template.Application.UseCases.Queries;
// 3. O Handler: A lógica do caso de uso.
public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    // Injetando o ILogger
    public GetProductByIdQueryHandler(IProductRepository productRepository, ILogger<GetProductByIdQueryHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando produto com ID: {ProductId}", request.Id);

        var product = await _productRepository.GetByIdAsync(request.Id);

        if (product is null)
        {
            _logger.LogWarning("Produto com ID: {ProductId} não foi encontrado", request.Id);
            throw new Exception($"Product with Id '{request.Id}' was not found.");
        }

        _logger.LogInformation("Produto {ProductId} encontrado com sucesso", request.Id);

        return new ProductResponse(product.Id, product.Name, product.Price);
    }
}