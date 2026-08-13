using Microsoft.EntityFrameworkCore;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllers();

// EF Core with SQL Server
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    )
);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Todo API",
        Version = "v1",
        Description = "ASP.NET Core Web API for managing Todo items with SQL Server"
    });
});
 
// ── App pipeline ───────────────────────────────────────────────────────────────

var app = builder.Build();

// Apply pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        c.RoutePrefix = string.Empty; // serve Swagger at root "/"
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod());

app.Run();
