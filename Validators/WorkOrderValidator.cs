using WebApplication1.Interfaces;
using WebApplication1.Models.Enums;

namespace WebApplication1.Validators;

public class WorkOrderValidator : IWorkOrderValidator
{
    public bool TryParseStatus(string? status, out WorkOrderStatus parsedStatus)
    {
        return Enum.TryParse(status, true, out parsedStatus);
    }

    public bool IsValidTransition(WorkOrderStatus current, WorkOrderStatus next)
    {
        return (current, next) switch
        {
            (WorkOrderStatus.Pending, WorkOrderStatus.InProgress) => true,
            (WorkOrderStatus.Pending, WorkOrderStatus.Failed) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.Failed) => true,
            _ => false
        };
    }
}
