using BoardTaskService.Application.TaskModule.Command.Update;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.ServiceUnitTests;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Update;

public class UpdateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateTaskAndReturnDetails()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        TaskRequestDto request = TestData.TaskRequest();
        var expected = new TaskDetailsDto { Id = taskId, Title = request.Title };
        var service = new Mock<ITaskService>();
        var logger = new Mock<ILoggerManager<UpdateTaskCommandHandler>>();

        service.Setup(x => x.UpdateTask(boardId, taskId, request, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new UpdateTaskCommandHandler(service.Object, logger.Object);

        TaskDetailsDto actual = await handler.Handle(new UpdateTaskCommand(boardId, taskId, request), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingRequiredValues()
    {
        var result = new UpdateTaskCommandValidator().Validate(
            new UpdateTaskCommand(Guid.Empty, Guid.Empty, new TaskRequestDto()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "BoardId");
        result.Errors.Should().Contain(x => x.PropertyName == "TaskId");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task title is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Priority is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task type is required.");
        result.Errors.Should().Contain(x => x.PropertyName == "Request.WorkflowColumnId");
    }
}
