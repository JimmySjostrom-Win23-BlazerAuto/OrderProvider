using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class OrderEntity
{
    [Key]
    public int Id { get; set; }
    public string CourseId { get; set; } = null!;
    public string UserId { get; set; } = null!;
}