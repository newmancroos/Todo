# Todo API — Setup Guide

[![Build and Test — TodoApi](https://github.com/newmancroos/Todo/actions/workflows/BuildAndTest.yml/badge.svg?branch=main)](https://github.com/newmancroos/Todo/actions/workflows/BuildAndTest.yml)

## Prerequisites
- .NET 8 SDK
- SQL Server (local or remote) — LocalDB, SQL Server Express, or full instance all work

## 1. Configure the connection string

Edit `appsettings.json` (or `appsettings.Development.json` for local dev):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TodoDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Common connection string formats:

| Scenario | Connection string |
|---|---|
| Windows auth / LocalDB | `Server=(localdb)\\mssqllocaldb;Database=TodoDb;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Server Express | `Server=.\\SQLEXPRESS;Database=TodoDb;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL login | `Server=localhost;Database=TodoDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |

## 2. Apply migrations (two options)

### Option A — EF Core CLI (recommended)
```bash
cd TodoApi
dotnet tool install --global dotnet-ef  # skip if already installed
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Option B — Run the SQL script manually
Open `Migrations/InitialCreate.sql` in SSMS or Azure Data Studio and execute it against your SQL Server instance. The app auto-calls `db.Database.Migrate()` on startup, so the tables will also be created automatically on first run if you skip option A.

## 3. Run the API
```bash
cd TodoApi
dotnet run
```

The Swagger UI is served at `http://localhost:<port>/` (root).

---

## Endpoints

| Method | URL | Description |
|--------|-----|-------------|
| `GET` | `/api/todo` | Get all todos (optional `?status=Incomplete\|Complete\|Closed`) |
| `GET` | `/api/todo/{id}` | Get a single todo by ID |
| `POST` | `/api/todo` | Create a new todo |
| `PUT` | `/api/todo/{id}` | Update title and description |
| `PATCH` | `/api/todo/{id}/status` | Change status (Incomplete / Complete / Closed) |
| `DELETE` | `/api/todo/{id}` | Remove a todo |

---

## Example payloads

### Create
```json
POST /api/todo
{
  "title": "Buy groceries",
  "description": "Milk, eggs, bread"
}
```

### Update
```json
PUT /api/todo/1
{
  "title": "Buy groceries (updated)",
  "description": "Milk, eggs, bread, butter"
}
```

### Update status
```json
PATCH /api/todo/1/status
{
  "status": 1
}
```
Status values: `0` = Incomplete, `1` = Complete, `2` = Closed

---

## Project structure

```
TodoApi/
├── Controllers/
│   └── TodoController.cs       # All API endpoints
├── Data/
│   └── TodoDbContext.cs        # EF Core DbContext
├── DTOs/
│   ├── CreateTodoRequest.cs
│   ├── UpdateTodoRequest.cs
│   ├── UpdateStatusRequest.cs
│   └── TodoItemResponse.cs
├── Migrations/
│   └── InitialCreate.sql       # Manual SQL alternative
├── Models/
│   ├── TodoItem.cs             # Entity model
│   └── TodoStatus.cs           # Enum: Incomplete / Complete / Closed
├── Program.cs                  # App startup & DI configuration
├── appsettings.json
└── TodoApi.csproj
```


