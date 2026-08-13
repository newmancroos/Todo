using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public class CreateTodoRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? ExpectedCompleteDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
