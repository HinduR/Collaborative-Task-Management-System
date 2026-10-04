using BoardTaskService.Application.TaskModule.Command.Comment.Create;
using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Comment.Create;

public class CreateTaskCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCommentAndReturnDto()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        var request = new CommentRequestDto { Comment = "Looks good" };
        var expected = new TaskCommentDto { Id = Guid.NewGuid(), Comment = request.Comment };
        var service = new Mock<ITaskCommentHelperService>();
        var logger = new Mock<ILoggerManager<CreateTaskCommentCommandHandler>>();

        service.Setup(x => x.CreateComment(boardId, taskId, request, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new CreateTaskCommentCommandHandler(service.Object, logger.Object);

        TaskCommentDto actual = await handler.Handle(
            new CreateTaskCommentCommand(boardId, taskId, request),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingValues()
    {
        var result = new CreateTaskCommentCommandValidator().Validate(
            new CreateTaskCommentCommand(Guid.Empty, Guid.Empty, null!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Comment request is required.");
    }

    [Fact]
    public void Validator_ShouldFail_ForEmptyOrTooLongComment()
    {
        var validator = new CreateTaskCommentCommandValidator();

        validator.Validate(new CreateTaskCommentCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new CommentRequestDto()))
            .Errors.Should().Contain(x => x.ErrorMessage == "Comment is required.");

        validator.Validate(new CreateTaskCommentCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new CommentRequestDto { Comment = new string('a', 1001) }))
            .Errors.Should().Contain(x => x.ErrorMessage == "Comment cannot exceed 1000 characters.");
    }
}
