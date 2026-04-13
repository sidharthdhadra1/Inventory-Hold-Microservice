using InventoryHold.Contracts.DTOs;
using InventoryHold.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryHold.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HoldsController : ControllerBase
{
    private readonly HoldService _service;

    public HoldsController(HoldService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(HoldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    /// <summary>
    /// Creates a new hold.
    /// </summary>
    public async Task<IActionResult> Create([FromBody] HoldRequest req)
    {
        try
        {
            var res = await _service.CreateHold(req);
            return CreatedAtAction(nameof(Get), new { holdId = res.HoldId }, res);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{holdId}")]
    [ProducesResponseType(typeof(HoldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    /// <summary>
    /// Retrieves a hold by its ID.
    /// </summary>
    public async Task<IActionResult> Get(string holdId)
    {
        var res = await _service.GetHold(holdId);
        if (res == null) return NotFound();
        return Ok(res);
    }

    [HttpDelete("{holdId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    /// <summary>
    /// Releases a hold by its ID.
    /// </summary>
    public async Task<IActionResult> Release(string holdId)
    {
        try
        {
            await _service.ReleaseHold(holdId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
