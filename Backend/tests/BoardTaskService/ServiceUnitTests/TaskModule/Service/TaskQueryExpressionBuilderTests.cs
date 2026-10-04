using BoardTaskService.Application.TaskModule.Common.Constants;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.HelperService;
using BoardTaskService.Domain.Models;
using FluentAssertions;
using Shared.Exceptions.Infrastructure;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Service;

public class TaskQueryExpressionBuilderTests
{
    [Fact]
    public void Build_ShouldReturnTrueExpression_WhenNoConditionsAreSupplied()
    {
        Func<BoardTask, bool> predicate = TaskQueryExpressionBuilder.Build([]).Compile();

        predicate(new BoardTask { Title = "Task" }).Should().BeTrue();
    }

    [Theory]
    [InlineData("Fix login", TaskQueryOperators.Equals, "Fix login", true)]
    [InlineData("Fix login", TaskQueryOperators.NotEquals, "Build board", true)]
    [InlineData("Fix login", TaskQueryOperators.Contains, "login", true)]
    [InlineData("Fix login", TaskQueryOperators.NotContains, "board", true)]
    [InlineData("Fix login", TaskQueryOperators.StartsWith, "Fix", true)]
    [InlineData("Fix login", TaskQueryOperators.EndsWith, "login", true)]
    public void Build_ShouldCreateStringPredicates(string title, string queryOperator, string value, bool expected)
    {
        Func<BoardTask, bool> predicate = TaskQueryExpressionBuilder.Build(
            [Condition(TaskQueryFields.TaskTitle, queryOperator, value)]).Compile();

        predicate(new BoardTask { Title = title }).Should().Be(expected);
    }

    [Fact]
    public void Build_ShouldCreateStringEmptyPredicates()
    {
        TaskQueryConditionDto emptyCondition = Condition(TaskQueryFields.TaskTitle, TaskQueryOperators.IsEmpty, string.Empty);
        TaskQueryConditionDto notEmptyCondition = Condition(TaskQueryFields.TaskTitle, TaskQueryOperators.IsNotEmpty, string.Empty);

        TaskQueryExpressionBuilder.Build([emptyCondition]).Compile()(new BoardTask { Title = string.Empty }).Should().BeTrue();
        TaskQueryExpressionBuilder.Build([notEmptyCondition]).Compile()(new BoardTask { Title = "Task" }).Should().BeTrue();
    }

    [Theory]
    [InlineData(TaskQueryFields.Priority)]
    [InlineData(TaskQueryFields.TaskType)]
    [InlineData(TaskQueryFields.WorkflowColumn)]
    public void Build_ShouldCreateGuidPredicates(string field)
    {
        Guid id = Guid.NewGuid();
        Func<BoardTask, bool> equals = TaskQueryExpressionBuilder.Build(
            [Condition(field, TaskQueryOperators.Equals, id.ToString())]).Compile();
        Func<BoardTask, bool> notEquals = TaskQueryExpressionBuilder.Build(
            [Condition(field, TaskQueryOperators.NotEquals, Guid.NewGuid().ToString())]).Compile();
        var task = new BoardTask
        {
            Title = "Task",
            PriorityId = id,
            TaskTypeId = id,
            WorkflowColumnId = id
        };

        equals(task).Should().BeTrue();
        notEquals(task).Should().BeTrue();
    }

    [Fact]
    public void Build_ShouldCombineConditionsWithAndAndOr()
    {
        var conditions = new List<TaskQueryConditionDto>
        {
            Condition(TaskQueryFields.TaskTitle, TaskQueryOperators.Contains, "board"),
            Condition(TaskQueryFields.Priority, TaskQueryOperators.Equals, Guid.NewGuid().ToString()),
            Condition(TaskQueryFields.TaskType, TaskQueryOperators.Equals, Guid.NewGuid().ToString(), TaskQueryLogicalOperators.Or)
        };
        BoardTask matchingTask = new()
        {
            Title = "board task",
            PriorityId = Guid.Parse(conditions[1].Value),
            TaskTypeId = Guid.NewGuid()
        };

        TaskQueryExpressionBuilder.Build(conditions).Compile()(matchingTask).Should().BeTrue();
    }

    [Fact]
    public void Build_ShouldCreateAssigneePredicates()
    {
        Guid userId = Guid.NewGuid();
        BoardTask assignedTask = new()
        {
            Title = "Task",
            AssigneeBoardAccessId = Guid.NewGuid(),
            AssigneeBoardAccess = new BoardAccess
            {
                UserProjectMapping = new UserProjectMapping { UserId = userId }
            }
        };

        TaskQueryExpressionBuilder.Build([Condition(TaskQueryFields.AssignedTo, TaskQueryOperators.Equals, userId.ToString())])
            .Compile()(assignedTask).Should().BeTrue();
        TaskQueryExpressionBuilder.Build([Condition(TaskQueryFields.AssignedTo, TaskQueryOperators.NotEquals, Guid.NewGuid().ToString())])
            .Compile()(assignedTask).Should().BeTrue();
        TaskQueryExpressionBuilder.Build([Condition(TaskQueryFields.AssignedTo, TaskQueryOperators.IsNotEmpty, string.Empty)])
            .Compile()(assignedTask).Should().BeTrue();
        TaskQueryExpressionBuilder.Build([Condition(TaskQueryFields.AssignedTo, TaskQueryOperators.IsEmpty, string.Empty)])
            .Compile()(new BoardTask { Title = "Task" }).Should().BeTrue();
    }

    [Fact]
    public void Build_ShouldThrow_ForInvalidFieldOperatorAndGuid()
    {
        Action invalidField = () => TaskQueryExpressionBuilder.Build([Condition("NOPE", TaskQueryOperators.Equals, "x")]);
        Action invalidOperator = () => TaskQueryExpressionBuilder.Build([Condition(TaskQueryFields.TaskTitle, "NOPE", "x")]);
        Action invalidGuid = () => TaskQueryExpressionBuilder.Build([Condition(TaskQueryFields.Priority, TaskQueryOperators.Equals, "x")]);

        invalidField.Should().Throw<BadRequestCustomException>();
        invalidOperator.Should().Throw<BadRequestCustomException>();
        invalidGuid.Should().Throw<BadRequestCustomException>();
    }

    private static TaskQueryConditionDto Condition(
        string field,
        string queryOperator,
        string value,
        string logicalOperator = TaskQueryLogicalOperators.And)
    {
        return new TaskQueryConditionDto
        {
            Field = field,
            Operator = queryOperator,
            Value = value,
            LogicalOperator = logicalOperator
        };
    }
}
