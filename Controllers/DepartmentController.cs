using abcomm_test.Data;
using abcomm_test.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace abcomm_test.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartmentController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Department>>> GetAll()
    {
        return await db.Departments.AsNoTracking().ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Department>> Get(Guid id)
    {
        var department = await db.Departments.AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == id);
        if (department is null)
        {
            return NotFound();
        }

        return department;
    }

    [HttpPost]
    public async Task<ActionResult<Department>> Create(Department department)
    {
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = department.Id }, department);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Department department)
    {
        if (id != department.Id)
        {
            return BadRequest();
        }

        var existingDepartment = await db.Departments.FindAsync(id);
        if (existingDepartment is null)
        {
            return NotFound();
        }

        existingDepartment.Name = department.Name;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var department = await db.Departments.FindAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        if (await db.CrmTasks.AnyAsync(task => task.DepartmentId == id))
        {
            return Conflict(new { error = "Неможливо видалити департамент, який має призначені завдання." });
        }

        db.Departments.Remove(department);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
