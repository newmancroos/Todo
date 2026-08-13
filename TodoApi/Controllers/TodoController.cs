using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TodoController : ControllerBase
{
    private readonly TodoDbContext _context;

    public TodoController(TodoDbContext context)
    {
        _context = context;
    }

    // GET api/todo
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TodoItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TodoStatus? status = null)
    {
        var query = _context.TodoItems.AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToResponse(t))
            .ToListAsync();

        return Ok(items);
    }

    // GET api/todo/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item is null) return NotFound(new { message = $"Todo item {id} not found." });

        return Ok(MapToResponse(item));
    }

    // POST api/todo
    [HttpPost]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest request)
    {
        var item = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            Status = TodoStatus.Incomplete,
            ExpectedCompleteDate = request.ExpectedCompleteDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
    }

    // PUT api/todo/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTodoRequest request)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item is null) return NotFound(new { message = $"Todo item {id} not found." });

        item.Title = request.Title;
        item.Description = request.Description;
        item.ExpectedCompleteDate = request.ExpectedCompleteDate;
        item.Notes = request.Notes;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(item));
    }

    // PATCH api/todo/{id}/status
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item is null) return NotFound(new { message = $"Todo item {id} not found." });

        item.Status = request.Status;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(item));
    }

    // DELETE api/todo/{id}
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item is null) return NotFound(new { message = $"Todo item {id} not found." });

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static TodoItemResponse MapToResponse(TodoItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        Status = item.Status.ToString(),
        ExpectedCompleteDate = item.ExpectedCompleteDate,
        Notes = item.Notes,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
