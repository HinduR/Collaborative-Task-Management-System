using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.Query.Get;
using FluentAssertions;
using Moq;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Query.Get;

public class GetBoardTasksQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnBoardTasks()
    {
        Guid boardId = Guid.NewGuid();
        var expected = new List<TaskSummaryDto> { new() { Id = Guid.NewGuid(), Title = "Task" } };
        var service = new Mock<IBoardTaskListService>();

        service.Setup(x => x.GetBoardTasksAsync(boardId, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new GetBoardTasksQueryHandler(service.Object);

        List<TaskSummaryDto> actual = await handler.Handle(new GetBoardTasksQuery(boardId), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardIdIsEmpty()
    {
        var result = new GetBoardTasksQueryValidator().Validate(new GetBoardTasksQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "BoardId cannot be empty.");
    }
}
