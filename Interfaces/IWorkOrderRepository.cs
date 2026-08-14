using WebApplication1.Models.Request;
using WebApplication1.Models.Response;

namespace WebApplication1.Interfaces;

public interface IWorkOrderRepository
{
    int? Create(WorkOrderCreateRequest request);
    IEnumerable<WorkOrderResponse> GetAll();
    WorkOrderResponse GetById(int id);
    bool Delete(int id);
}
