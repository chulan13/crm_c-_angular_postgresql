using abcomm_test.Data;
using abcomm_test.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace abcomm_test.Controllers;

[ApiController]
[Route("[controller]")]
public class CrmTaskController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CrmTask>>> GetAll()
    {
        return await db.CrmTasks.AsNoTracking().ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CrmTask>> Get(Guid id)
    {
        var task = await db.CrmTasks.AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        return task;
    }

    [HttpPost]
    public async Task<ActionResult<CrmTask>> Create(CrmTask task)
    {
        if (await IsCancelledWithoutReason(task))
        {
            return BadRequest(new
            {
                error = "Необхідно вказати причину закриття, аби скасувати завдання."
            });
        }

        if (!await StatusExists(task.StatusId))
        {
            return BadRequest(new { error = "status_id must reference an existing status." });
        }

        if (!await DepartmentExists(task.DepartmentId))
        {
            return BadRequest(new { error = "department_id must reference an existing department." });
        }

        db.CrmTasks.Add(task);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CrmTask task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        if (await IsCancelledWithoutReason(task))
        {
            return BadRequest(new
            {
                error = "Необхідно вказати причину закриття, аби скасувати завдання."
            });
        }

        if (!await StatusExists(task.StatusId))
        {
            return BadRequest(new { error = "status_id must reference an existing status." });
        }

        if (!await DepartmentExists(task.DepartmentId))
        {
            return BadRequest(new { error = "department_id must reference an existing department." });
        }

        var existingTask = await db.CrmTasks.FindAsync(id);
        if (existingTask is null)
        {
            return NotFound();
        }

        db.Entry(existingTask).CurrentValues.SetValues(task);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var task = await db.CrmTasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        db.CrmTasks.Remove(task);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private Task<bool> DepartmentExists(Guid? departmentId)
    {
        return departmentId is null
            ? Task.FromResult(true)
            : db.Departments.AnyAsync(department => department.Id == departmentId);
    }

    private Task<bool> StatusExists(Guid? statusId)
    {
        return statusId is null
            ? Task.FromResult(true)
            : db.Statuses.AnyAsync(status => status.Id == statusId);
    }

    private Task<bool> IsCancelledWithoutReason(CrmTask task)
    {
        return string.IsNullOrWhiteSpace(task.CancelReason)
            ? db.Statuses.AnyAsync(status =>
                status.Id == task.StatusId && status.Name == "Скасовано")
            : Task.FromResult(false);
    }
}
