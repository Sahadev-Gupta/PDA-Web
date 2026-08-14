using WebApplication1.Models.Enums;
using WebApplication1.Validators;

namespace WebApplication1.Tests.Validators;

public class WorkOrderValidatorTests
{
    private readonly WorkOrderValidator _validator = new();

    [Fact]
    public void TryParseStatus_ValidStatus_ReturnsTrueAndParsedValue()
    {
        var parsed = _validator.TryParseStatus("InProgress", out var status);

        Assert.True(parsed);
        Assert.Equal(WorkOrderStatus.InProgress, status);
    }

    [Fact]
    public void TryParseStatus_InvalidStatus_ReturnsFalse()
    {
        var parsed = _validator.TryParseStatus("Unknown", out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData(WorkOrderStatus.Pending, WorkOrderStatus.InProgress, true)]
    [InlineData(WorkOrderStatus.Pending, WorkOrderStatus.Failed, true)]
    [InlineData(WorkOrderStatus.InProgress, WorkOrderStatus.Completed, true)]
    [InlineData(WorkOrderStatus.InProgress, WorkOrderStatus.Failed, true)]
    [InlineData(WorkOrderStatus.Completed, WorkOrderStatus.Pending, false)]
    [InlineData(WorkOrderStatus.Failed, WorkOrderStatus.InProgress, false)]
    public void IsValidTransition_ReturnsExpectedResult(WorkOrderStatus current, WorkOrderStatus next, bool expected)
    {
        var result = _validator.IsValidTransition(current, next);

        Assert.Equal(expected, result);
    }
}
