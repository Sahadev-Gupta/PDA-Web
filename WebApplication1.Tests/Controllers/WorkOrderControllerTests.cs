using Microsoft.AspNetCore.Mvc;
using WebApplication1.Controllers;
using WebApplication1.Interfaces;
using WebApplication1.Models.Request;
using WebApplication1.Models.Response;
using WebApplication1.Validators;

namespace WebApplication1.Tests.Controllers;

public class WorkOrderControllerTests
{
    [Fact]
    public async Task Get_ReturnsOk()
    {
        var repository = new FakeWorkOrderRepository { All = [] };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Get();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var repository = new FakeWorkOrderRepository { ById = _ => null };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.GetById(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<WorkOrderResponse>>(notFound.Value);
        Assert.False(payload.Success);
    }

    [Fact]
    public async Task Create_WhenRepositoryReturnsId_ReturnsCreated()
    {
        var repository = new FakeWorkOrderRepository { CreateResult = 10 };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var request = new WorkOrderCreateRequest
        {
            DeviceId = "D1",
            JobType = "Install",
            Location = "HQ"
        };

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedResult>(result);
        var payload = Assert.IsType<ApiResponse<WorkOrderResponse>>(created.Value);
        Assert.True(payload.Success);
    }

    [Fact]
    public async Task Create_WhenRepositoryReturnsNull_ReturnsBadRequest()
    {
        var repository = new FakeWorkOrderRepository { CreateResult = null };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Create(new WorkOrderCreateRequest());

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<WorkOrderResponse>>(badRequest.Value);
        Assert.False(payload.Success);
    }

    [Fact]
    public async Task Update_WhenOrderNotFound_ReturnsNotFound()
    {
        var repository = new FakeWorkOrderRepository { ById = _ => null };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Update(1, new WorkOrderUpdateRequest { Status = "InProgress" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenNewStatusInvalid_ReturnsBadRequest()
    {
        var repository = new FakeWorkOrderRepository
        {
            ById = _ => new WorkOrderResponse { Id = 1, Status = "Pending" }
        };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Update(1, new WorkOrderUpdateRequest { Status = "Invalid" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<WorkOrderResponse>>(badRequest.Value);
        Assert.False(payload.Success);
    }

    [Fact]
    public async Task Update_WhenExistingStatusInvalid_ReturnsBadRequest()
    {
        var repository = new FakeWorkOrderRepository
        {
            ById = _ => new WorkOrderResponse { Id = 1, Status = "Unknown" }
        };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Update(1, new WorkOrderUpdateRequest { Status = "InProgress" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<WorkOrderResponse>>(badRequest.Value);
        Assert.False(payload.Success);
    }

    [Fact]
    public async Task Update_WhenTransitionInvalid_ReturnsBadRequestWithMessage()
    {
        var repository = new FakeWorkOrderRepository
        {
            ById = _ => new WorkOrderResponse { Id = 1, Status = "Completed" }
        };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Update(1, new WorkOrderUpdateRequest { Status = "Pending" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var messageProperty = badRequest.Value?.GetType().GetProperty("message");
        var messageValue = messageProperty?.GetValue(badRequest.Value)?.ToString();

        Assert.NotNull(messageValue);
        Assert.Contains("Invalid transition", messageValue);
    }

    [Fact]
    public async Task Update_WhenTransitionValid_ReturnsOk()
    {
        var repository = new FakeWorkOrderRepository
        {
            ById = _ => new WorkOrderResponse { Id = 1, Status = "Pending" }
        };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.Update(1, new WorkOrderUpdateRequest { Status = "InProgress" });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteById_WhenDeleted_ReturnsOk()
    {
        var repository = new FakeWorkOrderRepository { DeleteResult = true };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.DeleteById(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteById_WhenNotDeleted_ReturnsBadRequest()
    {
        var repository = new FakeWorkOrderRepository { DeleteResult = false };
        var controller = new WorkOrderController(repository, new WorkOrderValidator());

        var result = await controller.DeleteById(1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private sealed class FakeWorkOrderRepository : IWorkOrderRepository
    {
        public int? CreateResult { get; set; }
        public bool DeleteResult { get; set; }
        public IEnumerable<WorkOrderResponse> All { get; set; } = [];
        public Func<int, WorkOrderResponse?> ById { get; set; } = _ => new WorkOrderResponse { Id = 1, Status = "Pending" };

        public int? Create(WorkOrderCreateRequest request) => CreateResult;

        public bool Delete(int id) => DeleteResult;

        public IEnumerable<WorkOrderResponse> GetAll() => All;

        public WorkOrderResponse GetById(int id) => ById(id)!;
    }
}
