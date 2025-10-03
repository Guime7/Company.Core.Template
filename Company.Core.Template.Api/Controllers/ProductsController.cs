using Company.Core.Template.Application.Features.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Company.Core.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _mediator; // ISender é a nova forma de usar o MediatR, focada apenas em enviar

    public ProductsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query);

        // No futuro, nosso middleware de exceção cuidará do caso de não encontrar, retornando 404.
        // Por enquanto, o resultado será retornado diretamente.
        return Ok(result);
    }
}