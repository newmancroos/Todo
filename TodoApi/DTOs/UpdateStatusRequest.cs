using System.ComponentModel.DataAnnotations;
using TodoApi.Models;

namespace TodoApi.DTOs;

public class UpdateStatusRequest
{
    [Required]
    [EnumDataType(typeof(TodoStatus))]
    public TodoStatus Status { get; set; }
}
