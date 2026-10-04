using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Application.BoardModule.Service;
using BoardTaskService.Domain.Models;
using FluentAssertions;
using Moq;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.BoardModule.Service;

public class WorkflowColumnHelperServiceTests
{
    [Fact]
    public async Task SaveWorkflowColumnsAsync_ShouldCreateUpdateRemoveAndReturnDtos()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid existingId = Guid.NewGuid();
        Guid removedId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        WorkflowColumn existing = new()
        {
            Id = existingId,
            BoardId = boardId,
            Name = "Todo",
            SortOrder = 2,
            RoleId = []
        };
        WorkflowColumn removed = new()
        {
            Id = removedId,
            BoardId = boardId,
            Name = "Done",
            SortOrder = 1,
            IsActive = true
        };
        var request = new UpdateWorkflowColumnsRequest
        {
            WorkflowColumnList =
            [
                new WorkflowColumnRequest { Id = existingId, ColumnName = "  Doing  ", RoleIds = [Guid.Empty, roleId, roleId] },
                new WorkflowColumnRequest { ColumnName = "Review" }
            ]
        };
        var workflowService = new Mock<IWorkflowColumnService>();
        var service = CreateService(workflowService, userId);

        workflowService.Setup(x => x.GetActiveBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync(new Board { Id = boardId, Name = "Board", CreatedBy = userId });
        workflowService.Setup(x => x.GetActiveColumnsByBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync([existing, removed]);
        workflowService.Setup(x => x.HasActiveTasksInColumnsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Single() == removedId),
                CancellationToken.None))
            .ReturnsAsync(false);

        List<WorkflowColumnDto> result = await service.SaveWorkflowColumnsAsync(boardId, request, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].ColumnName.Should().Be("Doing");
        result[0].SortOrder.Should().Be(1);
        result[0].RoleIds.Should().Equal(roleId);
        result[1].ColumnName.Should().Be("Review");
        removed.IsActive.Should().BeFalse();
        workflowService.Verify(x => x.PersistColumnChangesAsync(
            It.Is<List<WorkflowColumn>>(columns => columns.Single() == existing),
            It.Is<List<WorkflowColumn>>(columns => columns.Single() == removed),
            It.Is<List<WorkflowColumn>>(columns => columns.Single().Name == "Review"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SaveWorkflowColumnsAsync_ShouldThrow_WhenBoardIsMissingOrUserIsNotOwner()
    {
        Guid boardId = Guid.NewGuid();
        var workflowService = new Mock<IWorkflowColumnService>();
        var service = CreateService(workflowService, Guid.NewGuid());
        var request = RequestWithOneColumn();

        await service.Invoking(x => x.SaveWorkflowColumnsAsync(boardId, request, CancellationToken.None))
            .Should().ThrowAsync<NotFoundCustomException>();

        workflowService.Setup(x => x.GetActiveBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync(new Board { Id = boardId, Name = "Board", CreatedBy = Guid.NewGuid() });

        await service.Invoking(x => x.SaveWorkflowColumnsAsync(boardId, request, CancellationToken.None))
            .Should().ThrowAsync<ForBiddenCustomException>();
    }

    [Fact]
    public async Task SaveWorkflowColumnsAsync_ShouldThrow_WhenRequestReferencesUnknownColumn()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        var workflowService = new Mock<IWorkflowColumnService>();
        var service = CreateService(workflowService, userId);

        workflowService.Setup(x => x.GetActiveBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync(new Board { Id = boardId, Name = "Board", CreatedBy = userId });
        workflowService.Setup(x => x.GetActiveColumnsByBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync([]);

        await service.Invoking(x => x.SaveWorkflowColumnsAsync(
                boardId,
                new UpdateWorkflowColumnsRequest
                {
                    WorkflowColumnList = [new WorkflowColumnRequest { Id = Guid.NewGuid(), ColumnName = "Todo" }]
                },
                CancellationToken.None))
            .Should().ThrowAsync<BadRequestCustomException>();
    }

    [Fact]
    public async Task SaveWorkflowColumnsAsync_ShouldThrow_WhenRemovedColumnContainsActiveTasks()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid removedId = Guid.NewGuid();
        var workflowService = new Mock<IWorkflowColumnService>();
        var service = CreateService(workflowService, userId);

        workflowService.Setup(x => x.GetActiveBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync(new Board { Id = boardId, Name = "Board", CreatedBy = userId });
        workflowService.Setup(x => x.GetActiveColumnsByBoardAsync(boardId, CancellationToken.None))
            .ReturnsAsync([new WorkflowColumn { Id = removedId, BoardId = boardId, Name = "Todo", SortOrder = 1 }]);
        workflowService.Setup(x => x.HasActiveTasksInColumnsAsync(It.IsAny<IEnumerable<Guid>>(), CancellationToken.None))
            .ReturnsAsync(true);

        await service.Invoking(x => x.SaveWorkflowColumnsAsync(boardId, RequestWithOneColumn(), CancellationToken.None))
            .Should().ThrowAsync<BadRequestCustomException>();
    }

    private static WorkflowColumnHelperService CreateService(
        Mock<IWorkflowColumnService> workflowService,
        Guid userId)
    {
        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.GetUserId()).Returns(userId);

        return new WorkflowColumnHelperService(
            workflowService.Object,
            userContext.Object,
            Mock.Of<ILoggerManager<WorkflowColumnHelperService>>());
    }

    private static UpdateWorkflowColumnsRequest RequestWithOneColumn()
    {
        return new UpdateWorkflowColumnsRequest
        {
            WorkflowColumnList = [new WorkflowColumnRequest { ColumnName = "Todo" }]
        };
    }
}
