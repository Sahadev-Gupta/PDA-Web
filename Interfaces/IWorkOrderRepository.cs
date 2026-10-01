using WebApplication1.Models.Request;
using WebApplication1.Models.Response;

namespace WebApplication1.Interfaces;

public interface IWorkOrderRepository
{
    Task<int?> Create(WorkOrderCreateRequest request);
    Task<List<WorkOrderResponse>> GetAll();
    Task<WorkOrderResponse?> GetById(int id);
    Task<bool> Delete(int id);
}
