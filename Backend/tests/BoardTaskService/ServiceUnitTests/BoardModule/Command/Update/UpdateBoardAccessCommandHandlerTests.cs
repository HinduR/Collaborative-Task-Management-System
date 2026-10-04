using BoardTaskService.Application.BoardModule.Command.Update;
using BoardTaskService.Application.BoardModule.Contract.IService;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Command.Update;

public class UpdateBoardAccessCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReplaceBoardAccess()
    {
        Guid boardId = Guid.NewGuid();
        var mappingIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var service = new Mock<IBoardAccessService>();
        var logger = new Mock<ILoggerManager<UpdateBoardAccessCommandHandler>>();
        var handler = new UpdateBoardAccessCommandHandler(service.Object, logger.Object);

        await handler.Handle(new UpdateBoardAccessCommand(boardId, mappingIds), CancellationToken.None);

        service.Verify(x => x.ReplaceBoardAccess(boardId, mappingIds, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardIdIsEmpty()
    {
        var validator = new UpdateBoardAccessCommandValidator();

        var result = validator.Validate(new UpdateBoardAccessCommand(Guid.Empty, [Guid.NewGuid()]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "BoardId cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenMappingIdsAreNull()
    {
        var validator = new UpdateBoardAccessCommandValidator();

        var result = validator.Validate(new UpdateBoardAccessCommand(Guid.NewGuid(), null!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "UserProjectMappingIds cannot be null.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenMappingIdsContainEmptyOrDuplicates()
    {
        Guid duplicateId = Guid.NewGuid();
        var validator = new UpdateBoardAccessCommandValidator();

        var result = validator.Validate(new UpdateBoardAccessCommand(
            Guid.NewGuid(),
            [duplicateId, Guid.Empty, duplicateId]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "UserProjectMappingId cannot be empty.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Duplicate user-project mapping IDs are not allowed.");
    }
}
