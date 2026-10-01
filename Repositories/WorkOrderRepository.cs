using Microsoft.EntityFrameworkCore;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Models.Data;
using WebApplication1.Models.Enums;
using WebApplication1.Models.Request;
using WebApplication1.Models.Response;

namespace WebApplication1.Repositories;

public class WorkOrderRepository(ILogger<WorkOrderRepository> logger,
    AppDbContext dbContext,
    EncryptedAppDbContext? encryptedDbContext = null) : IWorkOrderRepository
{
    private readonly List<WorkOrder> _workOrders = new();

    private int _id = 1;

    public async Task<List<WorkOrderResponse>> GetAll()
    {
        return await dbContext.WorkOrders
                              .AsNoTracking()
                              .Select(wo => MapToResponse(wo))
                              .ToListAsync();
    }

    public async Task<WorkOrderResponse?> GetById(int id)
    {
        WorkOrder? workOrder = await dbContext.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(wo => wo.Id == id);

        return workOrder is null ? null : MapToResponse(workOrder);
    }

    public async Task<int?> Create(WorkOrderCreateRequest request)
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
            dbContext.WorkOrders.Add(workOrder);
            await dbContext.SaveChangesAsync();

            if (encryptedDbContext is not null)
            {
                encryptedDbContext.WorkOrders.Add(workOrder);
                await encryptedDbContext.SaveChangesAsync();
            }

            _id++;
            return workOrder.Id;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, exception.Message);
            throw;
        }
    }

    public async Task<bool> Delete(int id)
    {
        WorkOrder? workOrder = await dbContext.WorkOrders
            .FirstOrDefaultAsync(wo => wo.Id == id);

        if (workOrder != null)
        {
            _workOrders.Remove(workOrder);
            dbContext.WorkOrders.Remove(workOrder);
            await dbContext.SaveChangesAsync();

            if (encryptedDbContext is not null)
            {
                var encryptedWorkOrder = await encryptedDbContext.WorkOrders
                    .FirstOrDefaultAsync(wo => wo.Id == id);

                if (encryptedWorkOrder is not null)
                {
                    encryptedDbContext.WorkOrders.Remove(encryptedWorkOrder);
                    await encryptedDbContext.SaveChangesAsync();
                }
            }

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
