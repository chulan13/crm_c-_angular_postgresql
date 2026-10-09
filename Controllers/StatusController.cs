using abcomm_test.Data;
using abcomm_test.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace abcomm_test.Controllers;

[ApiController]
[Route("[controller]")]
public class StatusController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Status>>> GetAll()
    {
        return await db.Statuses.AsNoTracking().ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Status>> Get(Guid id)
    {
        var status = await db.Statuses.AsNoTracking()
            .FirstOrDefaultAsync(status => status.Id == id);
        if (status is null)
        {
            return NotFound();
        }

        return status;
    }

}
