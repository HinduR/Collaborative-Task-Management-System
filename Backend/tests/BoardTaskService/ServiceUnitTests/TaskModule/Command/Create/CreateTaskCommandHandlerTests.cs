using BoardTaskService.Application.TaskModule.Command.Create;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.ServiceUnitTests;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Create;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateTaskAndReturnSummary()
    {
        Guid boardId = Guid.NewGuid();
        TaskRequestDto request = TestData.TaskRequest();
        var expected = new TaskSummaryDto { Id = Guid.NewGuid(), Title = request.Title };
        var service = new Mock<ITaskService>();
        var logger = new Mock<ILoggerManager<CreateTaskCommandHandler>>();

        service.Setup(x => x.CreateTask(boardId, request, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new CreateTaskCommandHandler(service.Object, logger.Object);

        TaskSummaryDto actual = await handler.Handle(new CreateTaskCommand(boardId, request), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingBoardIdTitlePriorityAndType()
    {
        var result = new CreateTaskCommandValidator().Validate(
            new CreateTaskCommand(Guid.Empty, new TaskRequestDto()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task title is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Priority is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task type is required.");
    }

    [Fact]
    public void Validator_ShouldFail_ForTitleExceedingMaximumLength()
    {
        TaskRequestDto request = TestData.TaskRequest();
        request.Title = new string('a', 201);

        var result = new CreateTaskCommandValidator().Validate(new CreateTaskCommand(Guid.NewGuid(), request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task title cannot exceed 200 characters.");
    }
}
