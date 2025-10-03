using Company.Core.Template.Application.Features.Products;
using Company.Core.Template.Application.Common.CustomMediator; // <-- Usando nosso namespace
using Microsoft.AspNetCore.Mvc;

namespace Company.Core.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // A única mudança é aqui: de ISender para IMediator
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) // E aqui
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var query = new GetProductByIdQuery(id);

        // Esta linha não muda NADA!
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}

//8a2e771c-74a9-4a8a-a53a-9c7b3b3a0f1b