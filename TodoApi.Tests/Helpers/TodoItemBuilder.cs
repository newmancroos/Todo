using TodoApi.Models;

namespace TodoApi.Tests.Helpers;

/// <summary>
/// Fluent builder for creating TodoItem test fixtures.
/// </summary>
public class TodoItemBuilder
{
    private string _title = "Test Todo";
    private string? _description = "Test description";
    private TodoStatus _status = TodoStatus.Incomplete;
    private DateTime? _expectedCompleteDate = null;
    private string? _notes = null;

    public TodoItemBuilder WithTitle(string title) { _title = title; return this; }
    public TodoItemBuilder WithDescription(string? description) { _description = description; return this; }
    public TodoItemBuilder WithStatus(TodoStatus status) { _status = status; return this; }
    public TodoItemBuilder WithExpectedCompleteDate(DateTime? date) { _expectedCompleteDate = date; return this; }
    public TodoItemBuilder WithNotes(string? notes) { _notes = notes; return this; }

    public TodoItem Build() => new()
    {
        Title = _title,
        Description = _description,
        Status = _status,
        ExpectedCompleteDate = _expectedCompleteDate,
        Notes = _notes,
        CreatedAt = DateTime.UtcNow
    };
}
