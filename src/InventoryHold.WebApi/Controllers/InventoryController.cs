using InventoryHold.Domain.Repositories;
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
    public async Task<IActionResult> List()
    {
        var list = await _repo.ListAll();
        return Ok(list);
    }
}
