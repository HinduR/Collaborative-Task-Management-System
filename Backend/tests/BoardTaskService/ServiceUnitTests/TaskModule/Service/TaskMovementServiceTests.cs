using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.Service;
using BoardTaskService.Domain.Models;
using FluentAssertions;
using Moq;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Service;

public class TaskMovementServiceTests
{
    [Fact]
    public async Task MoveTask_ShouldMoveTaskSaveAndSendRealtimeUpdate()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid fromColumnId = Guid.NewGuid();
        Guid toColumnId = Guid.NewGuid();
        BoardTask task = new() { Id = taskId, Title = "Task", WorkflowColumnId = fromColumnId };
        var dataService = new Mock<ITaskMovementDataService>();
        var realtimeService = new Mock<IBoardRealtimeService>();
        var service = CreateService(dataService, realtimeService, userId);

        SetupValidMovement(dataService, boardId, userId, taskId, toColumnId, task);

        MoveTaskResponseDto result = await service.MoveTask(boardId, taskId, toColumnId, CancellationToken.None);

        result.TaskId.Should().Be(taskId);
        result.WorkflowColumnId.Should().Be(toColumnId);
        task.UpdatedBy.Should().Be(userId);
        dataService.Verify(x => x.SaveTask(task, CancellationToken.None), Times.Once);
        realtimeService.Verify(x => x.SendTaskMoved(
            It.Is<TaskMovedEventDto>(moved => moved.TaskId == taskId && moved.PreviousWorkflowColumnId == fromColumnId),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task MoveTask_ShouldReturnWithoutSaving_WhenTaskIsAlreadyInDestination()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid columnId = Guid.NewGuid();
        BoardTask task = new() { Id = taskId, Title = "Task", WorkflowColumnId = columnId };
        var dataService = new Mock<ITaskMovementDataService>();
        var realtimeService = new Mock<IBoardRealtimeService>();
        var service = CreateService(dataService, realtimeService, userId);

        SetupValidMovement(dataService, boardId, userId, taskId, columnId, task);

        MoveTaskResponseDto result = await service.MoveTask(boardId, taskId, columnId, CancellationToken.None);

        result.WorkflowColumnId.Should().Be(columnId);
        dataService.Verify(x => x.SaveTask(It.IsAny<BoardTask>(), It.IsAny<CancellationToken>()), Times.Never);
        realtimeService.Verify(x => x.SendTaskMoved(It.IsAny<TaskMovedEventDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MoveTask_ShouldNotFail_WhenRealtimeUpdateFailsAfterSave()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid toColumnId = Guid.NewGuid();
        BoardTask task = new() { Id = taskId, Title = "Task", WorkflowColumnId = Guid.NewGuid() };
        var dataService = new Mock<ITaskMovementDataService>();
        var realtimeService = new Mock<IBoardRealtimeService>();
        var service = CreateService(dataService, realtimeService, userId);

        SetupValidMovement(dataService, boardId, userId, taskId, toColumnId, task);
        realtimeService
            .Setup(x => x.SendTaskMoved(It.IsAny<TaskMovedEventDto>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("signalr down"));

        MoveTaskResponseDto result = await service.MoveTask(boardId, taskId, toColumnId, CancellationToken.None);

        result.WorkflowColumnId.Should().Be(toColumnId);
        dataService.Verify(x => x.SaveTask(task, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task MoveTask_ShouldThrow_WhenAccessTaskOrColumnValidationFails()
    {
        Guid userId = Guid.NewGuid();
        var dataService = new Mock<ITaskMovementDataService>();
        var realtimeService = new Mock<IBoardRealtimeService>();
        var service = CreateService(dataService, realtimeService, userId);

        await service.Invoking(x => x.MoveTask(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<ForBiddenCustomException>();

        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        dataService.Setup(x => x.HasBoardAccess(boardId, userId, CancellationToken.None)).ReturnsAsync(true);

        await service.Invoking(x => x.MoveTask(boardId, taskId, Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<NotFoundCustomException>();

        BoardTask task = new() { Id = taskId, Title = "Task", WorkflowColumnId = Guid.NewGuid() };
        dataService.Setup(x => x.GetTask(boardId, taskId, CancellationToken.None)).ReturnsAsync(task);

        await service.Invoking(x => x.MoveTask(boardId, taskId, Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<BadRequestCustomException>();
    }

    private static TaskMovementService CreateService(
        Mock<ITaskMovementDataService> dataService,
        Mock<IBoardRealtimeService> realtimeService,
        Guid userId)
    {
        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.GetUserId()).Returns(userId);

        return new TaskMovementService(
            dataService.Object,
            realtimeService.Object,
            userContext.Object,
            Mock.Of<ILoggerManager<TaskMovementService>>());
    }

    private static void SetupValidMovement(
        Mock<ITaskMovementDataService> dataService,
        Guid boardId,
        Guid userId,
        Guid taskId,
        Guid workflowColumnId,
        BoardTask task)
    {
        dataService.Setup(x => x.HasBoardAccess(boardId, userId, CancellationToken.None)).ReturnsAsync(true);
        dataService.Setup(x => x.GetTask(boardId, taskId, CancellationToken.None)).ReturnsAsync(task);
        dataService.Setup(x => x.WorkflowColumnExists(boardId, workflowColumnId, CancellationToken.None)).ReturnsAsync(true);
    }
}
