using System.Linq.Expressions;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.Query.TaskQuery;
using BoardTaskService.Domain.Models;
using FluentAssertions;
using Moq;
using Shared.Common.contracts;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Query.TaskQuery;

public class GetTaskQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldQueryTasksForCurrentUser()
    {
        Guid userId = Guid.NewGuid();
        var queryRequest = new TaskQueryRequestDto
        {
            PageNumber = 2,
            PageSize = 10,
            Conditions = []
        };
        var expected = new TaskQueryResponseDto
        {
            Items = [new TaskDetailsDto { Id = Guid.NewGuid(), Title = "Task" }],
            TotalCount = 1
        };
        var service = new Mock<ITaskQueryService>();
        var userContext = new Mock<IUserContext>();
        var logger = new Mock<ILoggerManager<GetTaskQueryHandler>>();

        userContext.Setup(x => x.GetUserId()).Returns(userId);
        service
            .Setup(x => x.QueryTasksAsync(
                userId,
                It.IsAny<Expression<Func<BoardTask, bool>>>(),
                queryRequest.PageNumber,
                queryRequest.PageSize,
                CancellationToken.None))
            .ReturnsAsync(expected);

        var handler = new GetTaskQueryHandler(service.Object, userContext.Object, logger.Object);

        TaskQueryResponseDto actual = await handler.Handle(new GetTaskQuery(queryRequest), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_WhenRequestIsNull()
    {
        var result = new GetTaskQueryValidator().Validate(new GetTaskQuery(null!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task query request is required.");
    }

    [Theory]
    [InlineData(0, 20, "Page number must be greater than zero.")]
    [InlineData(1, 0, "Page size must be between 1 and 100.")]
    [InlineData(1, 101, "Page size must be between 1 and 100.")]
    public void Validator_ShouldFail_ForInvalidPagination(int pageNumber, int pageSize, string errorMessage)
    {
        var result = new GetTaskQueryValidator().Validate(new GetTaskQuery(new TaskQueryRequestDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == errorMessage);
    }
}
