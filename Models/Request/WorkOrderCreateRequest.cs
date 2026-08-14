using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Request
{
    public class WorkOrderCreateRequest
    {
        [Required]
        [StringLength(50)]
        public string? DeviceId { get; set; }

        [Required]
        [StringLength(100)]
        public string? JobType { get; set; }

        [Required]
        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? AssignedToUserId { get; set; }
    }
}
