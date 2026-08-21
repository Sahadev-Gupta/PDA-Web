using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Models.Enums;
using WebApplication1.Models.Request;
using WebApplication1.Models.Response;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderController(IWorkOrderRepository workOrderRepository, IWorkOrderValidator workOrderValidator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await workOrderRepository.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await workOrderRepository.GetById(id);
        var response = new ApiResponse<WorkOrderResponse>();
        if (result == null)
        {
            response.Success= false;
            response.Message = $"WorkOrder with ID {id} not found.";
            return NotFound(response);
        }

        response.Success = true;
        response.Data= result;
        return Ok(response);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] WorkOrderCreateRequest request)
    {
        var index = await workOrderRepository.Create(request);
        var response = new ApiResponse<WorkOrderResponse>();
        if (index!=null && index > 0)
        {
            response.Success = true;
            response.Message = "Work order created successfully.";
            return Created($"/api/workorders/{index}",
                response);
        }
        else
        {
            response.Success = false;
            response.Message = "Failed to create work order.";
            return BadRequest(response);
        }
        
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(int id, WorkOrderUpdateRequest request)
    {
        var order = await workOrderRepository.GetById(id);
        var response = new ApiResponse<WorkOrderResponse>();
        if (order == null)
        {
            response.Success = false;
            response.Message = $"WorkOrder with ID {id} not found.";
            return NotFound(response);
        }

        if (!workOrderValidator.TryParseStatus(request.Status, out var newStatus))
        {
            response.Success = false;
            response.Message = "Invalid status value";
            return BadRequest(response);
        }

        if (!workOrderValidator.TryParseStatus(order.Status, out var existingStatus))
        {
            response.Success = false;
            response.Message = "Existing work order status is invalid";
            return BadRequest(response);
        }

        if (!workOrderValidator.IsValidTransition(existingStatus, newStatus))
        {
            return BadRequest(new
            {
                message =
                    $"Invalid transition from {order.Status} to {newStatus}"
            });
        }

        return Ok();
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteById(int id) 
    { 
        var result = await workOrderRepository.Delete(id);
        var response = new ApiResponse<WorkOrderResponse>();
        if (result)
        {
            response.Success = result;
            response.Message = "Work order deleted successfully.";
            return Ok(response);
        }
        else
        {
            response.Success = result;
            response.Message = "Failed to delete work order.";
            return BadRequest(response);
        }
    }
}
