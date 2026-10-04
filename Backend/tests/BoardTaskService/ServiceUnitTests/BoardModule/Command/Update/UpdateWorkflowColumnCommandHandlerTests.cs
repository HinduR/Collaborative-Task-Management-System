using BoardTaskService.Application.BoardModule.Command.Update;
using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Command.Update;

public class UpdateWorkflowColumnCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldSaveWorkflowColumnsAndReturnDtos()
    {
        Guid boardId = Guid.NewGuid();
        var request = new UpdateWorkflowColumnsRequest
        {
            WorkflowColumnList = [new WorkflowColumnRequest { ColumnName = "Todo" }]
        };
        var expected = new List<WorkflowColumnDto> { new() { Id = Guid.NewGuid(), ColumnName = "Todo" } };
        var service = new Mock<IWorkflowColumnHelperService>();
        var logger = new Mock<ILoggerManager<UpdateWorkflowColumnCommandHandler>>();

        service
            .Setup(x => x.SaveWorkflowColumnsAsync(boardId, request, CancellationToken.None))
            .ReturnsAsync(expected);

        var handler = new UpdateWorkflowColumnCommandHandler(service.Object, logger.Object);

        List<WorkflowColumnDto> actual = await handler.Handle(
            new UpdateWorkflowColumnCommand(boardId, request),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForInvalidBoardId()
    {
        var validator = new UpdateWorkflowColumnCommandValidator();

        var result = validator.Validate(new UpdateWorkflowColumnCommand(
            Guid.Empty,
            new UpdateWorkflowColumnsRequest { WorkflowColumnList = [new WorkflowColumnRequest { ColumnName = "Todo" }] }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "BoardId cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_ForEmptyTooManyDuplicateNamesAndDuplicateIds()
    {
        Guid columnId = Guid.NewGuid();
        var validator = new UpdateWorkflowColumnCommandValidator();
        var request = new UpdateWorkflowColumnsRequest
        {
            WorkflowColumnList =
            [
                new WorkflowColumnRequest { Id = columnId, ColumnName = "Todo" },
                new WorkflowColumnRequest { Id = columnId, ColumnName = "todo" },
                new WorkflowColumnRequest { ColumnName = "" },
                new WorkflowColumnRequest { ColumnName = "Doing" },
                new WorkflowColumnRequest { ColumnName = "Review" },
                new WorkflowColumnRequest { ColumnName = "Done" }
            ]
        };

        var result = validator.Validate(new UpdateWorkflowColumnCommand(Guid.NewGuid(), request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "A board can have a maximum of five workflow columns.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Workflow column name cannot be empty.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Duplicate workflow column names are not allowed.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Duplicate workflow column IDs are not allowed.");
    }
}
