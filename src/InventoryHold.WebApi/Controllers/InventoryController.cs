using InventoryHold.Domain.Repositories;
using InventoryHold.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InventoryHold.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository _repo;

    public InventoryController(IInventoryRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    /// <summary>
    /// Retrieves a list of all inventory items.
    /// </summary>
    /// <returns>A list of inventory items.</returns>
    [ProducesResponseType(typeof(IEnumerable<ProductInventory>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> List()
    {
        var list = await _repo.ListAll();
        return Ok(list);
    }
}
