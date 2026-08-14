using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Request;

public class WorkOrderUpdateRequest
{
    [Required]
    public string? Status { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
