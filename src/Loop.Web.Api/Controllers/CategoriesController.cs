using Loop.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loop.Web.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class CategoriesController(IDispatcher dispatcher) : ControllerBase
{

    [HttpGet("{MallId}")]

    public async Task<IActionResult> GetCategories(Guid MallId, CancellationToken cancellationToken)
    {
        var query = new Application.Categories.Query.GetCategories.Query(MallId);
        var result = await dispatcher.Dispatch(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

