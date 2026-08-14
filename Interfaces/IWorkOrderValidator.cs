using WebApplication1.Models.Enums;

namespace WebApplication1.Interfaces;

public interface IWorkOrderValidator
{
    bool TryParseStatus(string? status, out WorkOrderStatus parsedStatus);
    bool IsValidTransition(WorkOrderStatus current, WorkOrderStatus next);
}
