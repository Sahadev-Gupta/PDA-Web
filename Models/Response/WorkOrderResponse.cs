namespace WebApplication1.Models.Response;

public class WorkOrderResponse
{
    public int Id { get; set; }

    public string? DeviceId { get; set; }

    public string? JobType { get; set; }

    public string? Status { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? AssignedToUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ProofOfDeliveryUrl { get; set; }

    public string? Notes { get; set; }
}
