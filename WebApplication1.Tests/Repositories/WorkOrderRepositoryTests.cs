using Microsoft.Extensions.Logging.Abstractions;
using WebApplication1.Models.Request;
using WebApplication1.Repositories;

namespace WebApplication1.Tests.Repositories;

public class WorkOrderRepositoryTests
{
    [Fact]
    public void Create_AssignsIncrementingIds()
    {
        var repository = new WorkOrderRepository(NullLogger<WorkOrderRepository>.Instance);

        var first = repository.Create(BuildRequest("D1"));
        var second = repository.Create(BuildRequest("D2"));

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void GetAll_ReturnsCreatedWorkOrders()
    {
        var repository = new WorkOrderRepository(NullLogger<WorkOrderRepository>.Instance);
        repository.Create(BuildRequest("D1"));
        repository.Create(BuildRequest("D2"));

        var all = repository.GetAll().ToList();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, w => w.DeviceId == "D1");
        Assert.Contains(all, w => w.DeviceId == "D2");
    }

    [Fact]
    public void GetById_ExistingId_ReturnsWorkOrder()
    {
        var repository = new WorkOrderRepository(NullLogger<WorkOrderRepository>.Instance);
        var id = repository.Create(BuildRequest("D1"));

        var item = repository.GetById(id!.Value);

        Assert.NotNull(item);
        Assert.Equal(id, item.Id);
        Assert.Equal("Pending", item.Status);
    }

    [Fact]
    public void Delete_ExistingId_ReturnsTrue()
    {
        var repository = new WorkOrderRepository(NullLogger<WorkOrderRepository>.Instance);
        var id = repository.Create(BuildRequest("D1"));

        var deleted = repository.Delete(id!.Value);

        Assert.True(deleted);
        Assert.Empty(repository.GetAll());
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalse()
    {
        var repository = new WorkOrderRepository(NullLogger<WorkOrderRepository>.Instance);

        var deleted = repository.Delete(999);

        Assert.False(deleted);
    }

    private static WorkOrderCreateRequest BuildRequest(string deviceId)
    {
        return new WorkOrderCreateRequest
        {
            DeviceId = deviceId,
            JobType = "Install",
            Location = "HQ",
            Description = "Desc",
            AssignedToUserId = "user-1"
        };
    }
}
