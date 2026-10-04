using BoardTaskService.Application.ProjectModule.Command.Update;
using BoardTaskService.Application.ProjectModule.Contract.IService;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.ProjectModule.Command.Update;

public class UpdateUserProjectMappingCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReplaceUserProjectMappings()
    {
        Guid userId = Guid.NewGuid();
        var projectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var service = new Mock<IProjectService>();
        var logger = new Mock<ILoggerManager<UpdateUserProjectMappingCommandHandler>>();
        var handler = new UpdateUserProjectMappingCommandHandler(service.Object, logger.Object);

        await handler.Handle(new UpdateUserProjectMappingCommand(userId, projectIds), CancellationToken.None);

        service.Verify(x => x.ReplaceUserProjectMappings(userId, projectIds, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_WhenUserIdIsEmpty()
    {
        var result = new UpdateUserProjectMappingCommandValidator().Validate(
            new UpdateUserProjectMappingCommand(Guid.Empty, [Guid.NewGuid()]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "UserId cannot be empty.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdsAreNull()
    {
        var result = new UpdateUserProjectMappingCommandValidator().Validate(
            new UpdateUserProjectMappingCommand(Guid.NewGuid(), null!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectIds cannot be null.");
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdsContainEmptyOrDuplicates()
    {
        Guid duplicateId = Guid.NewGuid();

        var result = new UpdateUserProjectMappingCommandValidator().Validate(
            new UpdateUserProjectMappingCommand(Guid.NewGuid(), [duplicateId, Guid.Empty, duplicateId]));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectId cannot be empty.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Duplicate project IDs are not allowed.");
    }
}
