using BoardTaskService.Application.BoardModule.Command.Delete;
using BoardTaskService.Application.BoardModule.Contract.IService;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Command.Delete;

public class DeleteBoardCommandHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldDeleteBoard(bool confirmDelete)
    {
        Guid projectId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        var boardService = new Mock<IBoardService>();
        var logger = new Mock<ILoggerManager<DeleteBoardCommandHandler>>();
        var cancellationToken = CancellationToken.None;

        var handler = new DeleteBoardCommandHandler(boardService.Object, logger.Object);

        await handler.Handle(
            new DeleteBoardCommand(projectId, boardId, confirmDelete),
            cancellationToken);

        boardService.Verify(
            x => x.DeleteBoard(projectId, boardId, confirmDelete, cancellationToken),
            Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdIsEmpty()
    {
        var validator = new DeleteBoardCommandValidator();

        var result = validator.Validate(new DeleteBoardCommand(
            Guid.Empty,
            Guid.NewGuid(),
            true));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectId cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardIdIsEmpty()
    {
        var validator = new DeleteBoardCommandValidator();

        var result = validator.Validate(new DeleteBoardCommand(
            Guid.NewGuid(),
            Guid.Empty,
            true));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "BoardId cannot be empty.");
    }
}
