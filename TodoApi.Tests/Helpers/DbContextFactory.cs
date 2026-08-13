using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

namespace TodoApi.Tests.Helpers;

/// <summary>
/// Creates an isolated, in-memory EF Core context for each test.
/// </summary>
public static class DbContextFactory
{
    public static TodoDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new TodoDbContext(options);
    }
}
