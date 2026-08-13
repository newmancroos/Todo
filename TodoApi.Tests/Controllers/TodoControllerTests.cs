using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Controllers;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Tests.Helpers;
using Xunit;

namespace TodoApi.Tests.Controllers;

public class TodoControllerTests : IDisposable
{
    private readonly TodoApi.Data.TodoDbContext _db;
    private readonly TodoController _sut;

    public TodoControllerTests()
    {
        _db = DbContextFactory.Create();
        _sut = new TodoController(_db);
    }

    public void Dispose() => _db.Dispose();

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<TodoItem> SeedAsync(Action<TodoItemBuilder>? configure = null)
    {
        var builder = new TodoItemBuilder();
        configure?.Invoke(builder);
        var item = builder.Build();
        _db.TodoItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    // ── GET ALL ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_NoItems_ReturnsEmptyList()
    {
        var result = await _sut.GetAll() as OkObjectResult;

        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Value.As<IEnumerable<TodoItemResponse>>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithItems_ReturnsAllItems()
    {
        await SeedAsync(b => b.WithTitle("Item A"));
        await SeedAsync(b => b.WithTitle("Item B"));

        var result = await _sut.GetAll() as OkObjectResult;
        var items = result!.Value.As<IEnumerable<TodoItemResponse>>();

        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_FilterByStatus_ReturnsMatchingItemsOnly()
    {
        await SeedAsync(b => b.WithStatus(TodoStatus.Incomplete));
        await SeedAsync(b => b.WithStatus(TodoStatus.Complete));
        await SeedAsync(b => b.WithStatus(TodoStatus.Closed));

        var result = await _sut.GetAll(status: TodoStatus.Complete) as OkObjectResult;
        var items = result!.Value.As<IEnumerable<TodoItemResponse>>();

        items.Should().HaveCount(1);
        items.Single().Status.Should().Be(nameof(TodoStatus.Complete));
    }

    // ── GET BY ID ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_ReturnsItem()
    {
        var seeded = await SeedAsync(b => b.WithTitle("Find me"));

        var result = await _sut.GetById(seeded.Id) as OkObjectResult;
        var response = result!.Value.As<TodoItemResponse>();

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Id.Should().Be(seeded.Id);
        response.Title.Should().Be("Find me");
    }

    [Fact]
    public async Task GetById_NonExistentId_Returns404()
    {
        var result = await _sut.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    // ── CREATE ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedItem()
    {
        var request = new CreateTodoRequest
        {
            Title = "New task",
            Description = "Details",
            ExpectedCompleteDate = DateTime.UtcNow.AddDays(7),
            Notes = "Some notes"
        };

        var result = await _sut.Create(request) as CreatedAtActionResult;
        var response = result!.Value.As<TodoItemResponse>();

        result.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Title.Should().Be("New task");
        response.Description.Should().Be("Details");
        response.Status.Should().Be(nameof(TodoStatus.Incomplete));
        response.Notes.Should().Be("Some notes");
        response.ExpectedCompleteDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ValidRequest_PersistsToDatabase()
    {
        var request = new CreateTodoRequest { Title = "Persisted task" };

        await _sut.Create(request);

        _db.TodoItems.Should().ContainSingle(t => t.Title == "Persisted task");
    }

    [Fact]
    public async Task Create_DefaultStatus_IsIncomplete()
    {
        var request = new CreateTodoRequest { Title = "Status check" };

        var result = await _sut.Create(request) as CreatedAtActionResult;
        var response = result!.Value.As<TodoItemResponse>();

        response.Status.Should().Be(nameof(TodoStatus.Incomplete));
    }

    // ── UPDATE ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingItem_ReturnsUpdatedItem()
    {
        var seeded = await SeedAsync(b => b.WithTitle("Original"));
        var request = new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "New desc",
            ExpectedCompleteDate = DateTime.UtcNow.AddDays(3),
            Notes = "Updated notes"
        };

        var result = await _sut.Update(seeded.Id, request) as OkObjectResult;
        var response = result!.Value.As<TodoItemResponse>();

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Title.Should().Be("Updated");
        response.Description.Should().Be("New desc");
        response.Notes.Should().Be("Updated notes");
        response.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        var request = new UpdateTodoRequest { Title = "Ghost" };

        var result = await _sut.Update(999, request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── UPDATE STATUS ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(TodoStatus.Complete)]
    [InlineData(TodoStatus.Incomplete)]
    [InlineData(TodoStatus.Closed)]
    public async Task UpdateStatus_ValidStatus_ReturnUpdatedStatus(TodoStatus newStatus)
    {
        var seeded = await SeedAsync();
        var request = new UpdateStatusRequest { Status = newStatus };

        var result = await _sut.UpdateStatus(seeded.Id, request) as OkObjectResult;
        var response = result!.Value.As<TodoItemResponse>();

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        response.Status.Should().Be(newStatus.ToString());
    }

    [Fact]
    public async Task UpdateStatus_NonExistentId_Returns404()
    {
        var request = new UpdateStatusRequest { Status = TodoStatus.Complete };

        var result = await _sut.UpdateStatus(999, request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateStatus_SetsUpdatedAt()
    {
        var seeded = await SeedAsync();
        seeded.UpdatedAt.Should().BeNull();

        await _sut.UpdateStatus(seeded.Id, new UpdateStatusRequest { Status = TodoStatus.Complete });

        var updated = await _db.TodoItems.FindAsync(seeded.Id);
        updated!.UpdatedAt.Should().NotBeNull();
    }

    // ── DELETE ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingItem_Returns204()
    {
        var seeded = await SeedAsync();

        var result = await _sut.Delete(seeded.Id);

        result.Should().BeOfType<NoContentResult>()
              .Which.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task Delete_ExistingItem_RemovesFromDatabase()
    {
        var seeded = await SeedAsync();

        await _sut.Delete(seeded.Id);

        _db.TodoItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        var result = await _sut.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
