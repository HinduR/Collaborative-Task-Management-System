using BoardTaskService.Application.BoardModule.Command.Create;
using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Command.Create;

public class CreateBoardCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateBoardAndReturnResponse()
    {
        Guid projectId = Guid.NewGuid();
        var request = new CreateBoardRequest { Name = "Sprint Board" };
        var expected = new CreateBoardResponseDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OwnerUserId = Guid.NewGuid()
        };
        var boardService = new Mock<IBoardService>();
        var logger = new Mock<ILoggerManager<CreateBoardCommandHandler>>();
        var cancellationToken = CancellationToken.None;

        boardService
            .Setup(x => x.CreateBoard(projectId, request, cancellationToken))
            .ReturnsAsync(expected);

        var handler = new CreateBoardCommandHandler(boardService.Object, logger.Object);

        CreateBoardResponseDto actual = await handler.Handle(
            new CreateBoardCommand(projectId, request),
            cancellationToken);

        actual.Should().BeSameAs(expected);
        boardService.Verify(
            x => x.CreateBoard(projectId, request, cancellationToken),
            Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdIsEmpty()
    {
        var validator = new CreateBoardCommandValidator();

        var result = validator.Validate(new CreateBoardCommand(
            Guid.Empty,
            new CreateBoardRequest { Name = "Sprint Board" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectId cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_ShouldFail_WhenBoardNameIsEmpty(string name)
    {
        var validator = new CreateBoardCommandValidator();

        var result = validator.Validate(new CreateBoardCommand(
            Guid.NewGuid(),
            new CreateBoardRequest { Name = name }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board name cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardNameExceedsMaximumLength()
    {
        var validator = new CreateBoardCommandValidator();

        var result = validator.Validate(new CreateBoardCommand(
            Guid.NewGuid(),
            new CreateBoardRequest { Name = new string('a', 151) }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board name cannot exceed 150 characters.");
    }
}
