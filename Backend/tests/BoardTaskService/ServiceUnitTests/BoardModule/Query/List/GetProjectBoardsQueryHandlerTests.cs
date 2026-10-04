using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Application.BoardModule.Query.List;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Query.List;

public class GetProjectBoardsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProjectBoards()
    {
        Guid projectId = Guid.NewGuid();
        var expected = new List<BoardSummaryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                BoardName = "Planning",
                OwnerUserId = Guid.NewGuid(),
                IsBoardOwner = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                BoardName = "Delivery",
                OwnerUserId = Guid.NewGuid(),
                IsBoardOwner = false
            }
        };
        var boardService = new Mock<IBoardService>();
        var logger = new Mock<ILoggerManager<GetProjectBoardsQueryHandler>>();
        var cancellationToken = CancellationToken.None;

        boardService
            .Setup(x => x.GetProjectBoards(projectId, cancellationToken))
            .ReturnsAsync(expected);

        var handler = new GetProjectBoardsQueryHandler(boardService.Object, logger.Object);

        List<BoardSummaryDto> actual = await handler.Handle(
            new GetProjectBoardsQuery(projectId),
            cancellationToken);

        actual.Should().BeSameAs(expected);
        boardService.Verify(
            x => x.GetProjectBoards(projectId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdIsEmpty()
    {
        var validator = new GetProjectBoardsQueryValidator();

        var result = validator.Validate(new GetProjectBoardsQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectId cannot be empty.");
    }
}
