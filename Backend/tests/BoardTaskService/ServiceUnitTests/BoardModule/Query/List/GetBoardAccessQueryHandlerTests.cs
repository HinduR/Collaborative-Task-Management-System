using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Application.BoardModule.Query.List;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Query.List;

public class GetBoardAccessQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnBoardAccessUsers()
    {
        Guid boardId = Guid.NewGuid();
        var expected = new List<BoardAccessUserDto> { new() { UserProjectMappingId = Guid.NewGuid() } };
        var service = new Mock<IBoardAccessService>();
        var logger = new Mock<ILoggerManager<GetBoardAccessQueryHandler>>();

        service.Setup(x => x.GetBoardAccess(boardId, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new GetBoardAccessQueryHandler(service.Object, logger.Object);

        List<BoardAccessUserDto> actual = await handler.Handle(new GetBoardAccessQuery(boardId), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_WhenBoardIdIsEmpty()
    {
        var result = new GetBoardAccessQueryValidator().Validate(new GetBoardAccessQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "BoardId cannot be empty.");
    }
}
