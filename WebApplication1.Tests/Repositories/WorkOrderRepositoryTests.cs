using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using WebApplication1.Models.Data;
using WebApplication1.Models.Request;
using WebApplication1.Repositories;

namespace WebApplication1.Tests.Repositories;

public class WorkOrderRepositoryTests
{
    [Test]
    public async Task Create_ValidRequest_ReturnsGeneratedIdAndAllowsGetById()
    {
        var (repository, dbContext) = CreateRepository();
        using (dbContext)
        {
            var request = new WorkOrderCreateRequest
            {
                DeviceId = "D-1",
                JobType = "Install",
                Location = "Plant A",
                Description = "Initial setup",
                AssignedToUserId = "user-1"
            };

            var id = await repository.Create(request);
            var workOrder = await repository.GetById(id!.Value);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(id, Is.EqualTo(1));
                Assert.That(workOrder.DeviceId, Is.EqualTo(request.DeviceId));
                Assert.That(workOrder.JobType, Is.EqualTo(request.JobType));
                Assert.That(workOrder.Location, Is.EqualTo(request.Location));
                Assert.That(workOrder.Status, Is.EqualTo("Pending"));
            }
        }
    }

    [Test]
    public async Task GetAll_WhenMultipleRecordsExist_ReturnsAllRecords()
    {
        var (repository, dbContext) = CreateRepository();
        using (dbContext)
        {
            await repository.Create(new WorkOrderCreateRequest { DeviceId = "D-1", JobType = "Install", Location = "A" });
            await repository.Create(new WorkOrderCreateRequest { DeviceId = "D-2", JobType = "Repair", Location = "B" });

            var result = await repository.GetAll();

            using (Assert.EnterMultipleScope())
            { 
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result.Select(x => x.DeviceId), Does.Contain("D-1"));
                Assert.That(result.Select(x => x.DeviceId), Does.Contain("D-2"));
            }
        }
    }

    [Test]
    public async Task Delete_ExistingId_ReturnsTrueAndRemovesRecord()
    {
        var (repository, dbContext) = CreateRepository();
        using (dbContext)
        {
            var id = await repository.Create(new WorkOrderCreateRequest { DeviceId = "D-1", JobType = "Install", Location = "A" });

            var deleted = await repository.Delete(id!.Value);
            var all = await repository.GetAll();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(deleted, Is.True);
                Assert.That(all, Is.Empty);
            }
        }
    }

    [Test]
    public async Task Delete_NonExistingId_ReturnsFalse()
    {
        var (repository, dbContext) = CreateRepository();
        using (dbContext)
        {
            var deleted = await repository.Delete(999);

            Assert.That(deleted, Is.False);
        }
    }

    [Test]
    public void GetById_NonExistingId_ThrowsNullReferenceException()
    {
        var (repository, dbContext) = CreateRepository();
        using (dbContext)
        {
            Assert.ThrowsAsync<NullReferenceException>(async () => await repository.GetById(999));
        }
    }

    [Test]
    public void Create_NullRequest_ThrowsExceptionAndLogsError()
    {
        var loggerMock = Substitute.For<ILogger<WorkOrderRepository>>();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .Options;

        using var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var repository = new WorkOrderRepository(loggerMock, dbContext);

        Assert.ThrowsAsync<NullReferenceException>(async () => await repository.Create(null!));

        var errorLogCalls = loggerMock.ReceivedCalls().Count(call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments().Length > 0
            && call.GetArguments()[0] is LogLevel level
            && level == LogLevel.Error);

        Assert.That(errorLogCalls, Is.EqualTo(1));
    }

    private static (WorkOrderRepository Repository, AppDbContext DbContext) CreateRepository()
    {
        var logger = Substitute.For<ILogger<WorkOrderRepository>>();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .Options;

        var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        return (new WorkOrderRepository(logger, dbContext), dbContext);
    }
}
