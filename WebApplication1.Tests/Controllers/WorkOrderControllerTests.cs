using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NUnit.Framework;
using WebApplication1.Controllers;
using WebApplication1.Interfaces;
using WebApplication1.Models.Enums;
using WebApplication1.Models.Request;
using WebApplication1.Models.Response;

namespace WebApplication1.Tests.Controllers;

public class WorkOrderControllerTests
{
    [Test]
    public async Task Get_WhenRepositoryReturnsData_ReturnsOkWithWorkOrders()
    {
        var data = new List<WorkOrderResponse>
        {
            new() { Id = 1, DeviceId = "D-1", JobType = "Install", Status = "Pending" }
        };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetAll().Returns(data);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Get();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.SameAs(data));
    }

    [Test]
    public async Task GetById_WhenWorkOrderExists_ReturnsOkWithApiResponse()
    {
        var workOrder = new WorkOrderResponse { Id = 1, DeviceId = "D-1", Status = "Pending" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(1).Returns(workOrder);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.GetById(1);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)okResult.Value!;
        Assert.That(response.Success, Is.True);
        Assert.That(response.Data, Is.SameAs(workOrder));
    }

    [Test]
    public async Task GetById_WhenWorkOrderDoesNotExist_ReturnsNotFound()
    {
        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(10).Returns((WorkOrderResponse)null!);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.GetById(10);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFound = (NotFoundObjectResult)result;
        Assert.That(notFound.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)notFound.Value!;
        Assert.That(response.Success, Is.False);
        Assert.That(response.Message, Does.Contain("not found").IgnoreCase);
    }

    [Test]
    public async Task Create_WhenRepositoryCreatesSuccessfully_ReturnsCreated()
    {
        var request = new WorkOrderCreateRequest { DeviceId = "D-1", JobType = "Install", Location = "Site" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.Create(request).Returns(1);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Create(request);

        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var created = (CreatedResult)result;
        Assert.That(created.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)created.Value!;
        Assert.That(response.Success, Is.True);
        Assert.That(response.Message, Is.EqualTo("Work order created successfully."));
    }

    [Test]
    public async Task Create_WhenRepositoryFails_ReturnsBadRequest()
    {
        var request = new WorkOrderCreateRequest { DeviceId = "D-1", JobType = "Install", Location = "Site" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.Create(request).Returns((int?)null);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Create(request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result;
        Assert.That(badRequest.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)badRequest.Value!;
        Assert.That(response.Success, Is.False);
        Assert.That(response.Message, Is.EqualTo("Failed to create work order."));
    }

    [Test]
    public async Task Update_WhenWorkOrderDoesNotExist_ReturnsNotFound()
    {
        var request = new WorkOrderUpdateRequest { Status = "InProgress" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(100).Returns((WorkOrderResponse)null!);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Update(100, request);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Update_WhenNewStatusIsInvalid_ReturnsBadRequest()
    {
        var request = new WorkOrderUpdateRequest { Status = "invalid" };
        var existing = new WorkOrderResponse { Id = 1, Status = "Pending" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(1).Returns(existing);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        validatorMock.TryParseStatus("invalid", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = default(WorkOrderStatus);
                return false;
            });

        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Update(1, request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result;
        Assert.That(badRequest.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)badRequest.Value!;
        Assert.That(response.Message, Is.EqualTo("Invalid status value"));
    }

    [Test]
    public async Task Update_WhenExistingStatusIsInvalid_ReturnsBadRequest()
    {
        var request = new WorkOrderUpdateRequest { Status = "InProgress" };
        var existing = new WorkOrderResponse { Id = 1, Status = "Unknown" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(1).Returns(existing);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        validatorMock.TryParseStatus("InProgress", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = WorkOrderStatus.InProgress;
                return true;
            });

        validatorMock.TryParseStatus("Unknown", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = default(WorkOrderStatus);
                return false;
            });

        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Update(1, request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result;
        Assert.That(badRequest.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)badRequest.Value!;
        Assert.That(response.Message, Is.EqualTo("Existing work order status is invalid"));
    }

    [Test]
    public async Task Update_WhenTransitionIsInvalid_ReturnsBadRequest()
    {
        var request = new WorkOrderUpdateRequest { Status = "Completed" };
        var existing = new WorkOrderResponse { Id = 1, Status = "Pending" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(1).Returns(existing);

        var validatorMock = Substitute.For<IWorkOrderValidator>();

        validatorMock.TryParseStatus("Completed", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = WorkOrderStatus.Completed;
                return true;
            });

        validatorMock.TryParseStatus("Pending", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = WorkOrderStatus.Pending;
                return true;
            });

        validatorMock.IsValidTransition(WorkOrderStatus.Pending, WorkOrderStatus.Completed).Returns(false);

        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Update(1, request);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WhenTransitionIsValid_ReturnsOk()
    {
        var request = new WorkOrderUpdateRequest { Status = "InProgress" };
        var existing = new WorkOrderResponse { Id = 1, Status = "Pending" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.GetById(1).Returns(existing);

        var validatorMock = Substitute.For<IWorkOrderValidator>();

        validatorMock.TryParseStatus("InProgress", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = WorkOrderStatus.InProgress;
                return true;
            });

        validatorMock.TryParseStatus("Pending", out Arg.Any<WorkOrderStatus>())
            .Returns(callInfo =>
            {
                callInfo[1] = WorkOrderStatus.Pending;
                return true;
            });

        validatorMock.IsValidTransition(WorkOrderStatus.Pending, WorkOrderStatus.InProgress).Returns(true);

        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.Update(1, request);

        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task DeleteById_WhenDeleteSucceeds_ReturnsOk()
    {
        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.Delete(1).Returns(true);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.DeleteById(1);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)ok.Value!;
        Assert.That(response.Success, Is.True);
    }

    [Test]
    public async Task DeleteById_WhenDeleteFails_ReturnsBadRequest()
    {
        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.Delete(1).Returns(false);

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        var result = await controller.DeleteById(1);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequest = (BadRequestObjectResult)result;
        Assert.That(badRequest.Value, Is.InstanceOf<ApiResponse<WorkOrderResponse>>());

        var response = (ApiResponse<WorkOrderResponse>)badRequest.Value!;
        Assert.That(response.Success, Is.False);
    }

    [Test]
    public void Create_WhenRepositoryThrowsException_ThrowsException()
    {
        var request = new WorkOrderCreateRequest { DeviceId = "D-1", JobType = "Install", Location = "Site" };

        var repositoryMock = Substitute.For<IWorkOrderRepository>();
        repositoryMock.Create(request).Returns(Task.FromException<int?>(new InvalidOperationException("unexpected")));

        var validatorMock = Substitute.For<IWorkOrderValidator>();
        var controller = new WorkOrderController(repositoryMock, validatorMock);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await controller.Create(request));
    }
}
