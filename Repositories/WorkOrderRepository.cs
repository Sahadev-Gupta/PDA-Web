using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Models.Enums;
using WebApplication1.Models.Request;
using WebApplication1.Models.Response;

namespace WebApplication1.Repositories;

public class WorkOrderRepository(ILogger<WorkOrderRepository> logger) : IWorkOrderRepository
{
    private readonly List<WorkOrder> _workOrders = new();

    private int _id = 1;

    public IEnumerable<WorkOrderResponse> GetAll()
    {
        return _workOrders
            .Select(wo => MapToResponse(wo));
    }

    public WorkOrderResponse GetById(int id)
    {
        WorkOrder? workOrder =  _workOrders
            .FirstOrDefault(wo => wo.Id == id);

        return MapToResponse(workOrder!);
    }

    public int? Create(WorkOrderCreateRequest request)
    {
        try
        {
            var workOrder = new WorkOrder
            {
                DeviceId = request.DeviceId,
                JobType = request.JobType,
                Location = request.Location,
                Description = request.Description,
                AssignedToUserId = request.AssignedToUserId,
                Status = WorkOrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Id = _id
            };
            _workOrders.Add(workOrder);
            _id++;

            MapToResponse(workOrder);
            return workOrder.Id;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, exception.Message);
            throw;
        }
    }

    public bool Delete(int id)
    {
        var workOrder = _workOrders.FirstOrDefault(wo => wo.Id == id);
        if (workOrder != null)
        {
            _workOrders.Remove(workOrder);
            return true;
        }
        return false;
    }

    private static WorkOrderResponse MapToResponse(WorkOrder workOrder)
    {
        return new WorkOrderResponse
        {
            Id = workOrder.Id,
            DeviceId = workOrder.DeviceId,
            JobType = workOrder.JobType,
            Status = workOrder.Status.ToString(),
            Location = workOrder.Location,
            Description = workOrder.Description,
            AssignedToUserId = workOrder.AssignedToUserId,
            CreatedAt = workOrder.CreatedAt,
            StartedAt = workOrder.StartedAt,
            CompletedAt = workOrder.CompletedAt,
            ProofOfDeliveryUrl = workOrder.ProofOfDeliveryUrl,
            Notes = workOrder.Notes
        };
    }
}
