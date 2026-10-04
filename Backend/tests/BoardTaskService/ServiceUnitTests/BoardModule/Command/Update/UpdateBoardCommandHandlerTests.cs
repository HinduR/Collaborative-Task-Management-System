using BoardTaskService.Application.BoardModule.Command.Update;
using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Command.Update;

public class UpdateBoardCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateBoardAndReturnSummary()
    {
        Guid projectId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        var request = new UpdateBoardRequest { BoardName = "Updated Board" };
        var expected = new BoardSummaryDto
        {
            Id = boardId,
            BoardName = request.BoardName,
            OwnerUserId = Guid.NewGuid(),
            IsBoardOwner = true
        };
        var boardService = new Mock<IBoardService>();
        var logger = new Mock<ILoggerManager<UpdateBoardCommandHandler>>();
        var cancellationToken = CancellationToken.None;

        boardService
            .Setup(x => x.UpdateBoard(projectId, boardId, request, cancellationToken))
            .ReturnsAsync(expected);

        var handler = new UpdateBoardCommandHandler(boardService.Object, logger.Object);

        BoardSummaryDto actual = await handler.Handle(
            new UpdateBoardCommand(projectId, boardId, request),
            cancellationToken);

        actual.Should().BeSameAs(expected);
        boardService.Verify(
            x => x.UpdateBoard(projectId, boardId, request, cancellationToken),
            Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdIsEmpty()
    {
        var validator = new UpdateBoardCommandValidator();

        var result = validator.Validate(new UpdateBoardCommand(
            Guid.Empty,
            Guid.NewGuid(),
            new UpdateBoardRequest { BoardName = "Updated Board" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectId cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardIdIsEmpty()
    {
        var validator = new UpdateBoardCommandValidator();

        var result = validator.Validate(new UpdateBoardCommand(
            Guid.NewGuid(),
            Guid.Empty,
            new UpdateBoardRequest { BoardName = "Updated Board" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "BoardId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_ShouldFail_WhenBoardNameIsEmpty(string boardName)
    {
        var validator = new UpdateBoardCommandValidator();

        var result = validator.Validate(new UpdateBoardCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdateBoardRequest { BoardName = boardName }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board name cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardNameExceedsMaximumLength()
    {
        var validator = new UpdateBoardCommandValidator();

        var result = validator.Validate(new UpdateBoardCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new UpdateBoardRequest { BoardName = new string('a', 151) }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board name cannot exceed 150 characters.");
    }
}
