using System.Linq.Expressions;
using BoardTaskService.Application.TaskModule.Common.Constants;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;
using Shared.Exceptions.Infrastructure;

namespace BoardTaskService.Application.TaskModule.HelperService;

/// <summary>Builds dynamic task-filter expressions from task query conditions.</summary>
public static class TaskQueryExpressionBuilder
{
    /// <summary>Builds a task-filter expression by combining the supplied conditions using their logical operators.</summary>
    /// <param name="conditions">The task query conditions to combine.</param>
    /// <returns>An expression that can be used to filter <see cref="BoardTask"/> entities.</returns>
    public static Expression<Func<BoardTask, bool>> Build(
        IReadOnlyCollection<TaskQueryConditionDto> conditions)
    {
        ParameterExpression taskParameter =
            Expression.Parameter(typeof(BoardTask), "task");

        Expression? combinedExpression = null;

        foreach (TaskQueryConditionDto condition in conditions)
        {
            Expression conditionExpression = BuildCondition(
                taskParameter,
                condition);

            if (combinedExpression is null)
            {
                // The first row's logical operator is ignored because
                // there is no previous condition to combine it with.
                combinedExpression = conditionExpression;
                continue;
            }

            combinedExpression = IsOr(condition.LogicalOperator)
                ? Expression.OrElse(
                    combinedExpression,
                    conditionExpression)
                : Expression.AndAlso(
                    combinedExpression,
                    conditionExpression);
        }

        combinedExpression ??= Expression.Constant(true);

        return Expression.Lambda<Func<BoardTask, bool>>(
            combinedExpression,
            taskParameter);
    }

    /// <summary>Builds the filter expression for an individual task query condition.</summary>
    /// <param name="task">The task parameter used by the expression.</param>
    /// <param name="condition">The task query condition to build.</param>
    /// <returns>The expression representing the supplied condition.</returns>
    private static Expression BuildCondition(
        ParameterExpression task,
        TaskQueryConditionDto condition)
    {
        string field = condition.Field
            .Trim()
            .ToUpperInvariant();

        return field switch
        {
            TaskQueryFields.TaskTitle => BuildStringCondition(
                Expression.Property(
                    task,
                    nameof(BoardTask.Title)),
                condition),

            TaskQueryFields.Priority => BuildGuidCondition(
                Expression.Property(
                    task,
                    nameof(BoardTask.PriorityId)),
                condition),

            TaskQueryFields.TaskType => BuildGuidCondition(
                Expression.Property(
                    task,
                    nameof(BoardTask.TaskTypeId)),
                condition),

            TaskQueryFields.WorkflowColumn => BuildGuidCondition(
                Expression.Property(
                    task,
                    nameof(BoardTask.WorkflowColumnId)),
                condition),

            TaskQueryFields.AssignedTo => BuildAssigneeUserCondition(
                task,
                condition),

            _ => throw new BadRequestCustomException(
                "Invalid query field.",
                string.Concat(
                    "The field '",
                    condition.Field,
                    "' is not supported."))
        };
    }

    /// <summary>Builds a string comparison expression for the supplied task query condition.</summary>
    /// <param name="property">The string property to compare.</param>
    /// <param name="condition">The task query condition containing the operator and comparison value.</param>
    /// <returns>The resulting string comparison expression.</returns>
    private static Expression BuildStringCondition(
        Expression property,
        TaskQueryConditionDto condition)
    {
        string value = condition.Value.Trim();
        Expression constant = Expression.Constant(value);

        string queryOperator = condition.Operator
            .Trim()
            .ToUpperInvariant();

        return queryOperator switch
        {
            TaskQueryOperators.Equals =>
                Expression.Equal(
                    property,
                    constant),

            TaskQueryOperators.NotEquals =>
                Expression.NotEqual(
                    property,
                    constant),

            TaskQueryOperators.Contains =>
                CallStringMethod(
                    property,
                    nameof(string.Contains),
                    constant),

            TaskQueryOperators.NotContains =>
                Expression.Not(
                    CallStringMethod(
                        property,
                        nameof(string.Contains),
                        constant)),

            TaskQueryOperators.StartsWith =>
                CallStringMethod(
                    property,
                    nameof(string.StartsWith),
                    constant),

            TaskQueryOperators.EndsWith =>
                CallStringMethod(
                    property,
                    nameof(string.EndsWith),
                    constant),

            TaskQueryOperators.IsEmpty =>
                Expression.OrElse(
                    Expression.Equal(
                        property,
                        Expression.Constant(
                            null,
                            typeof(string))),
                    Expression.Equal(
                        property,
                        Expression.Constant(string.Empty))),

            TaskQueryOperators.IsNotEmpty =>
                Expression.AndAlso(
                    Expression.NotEqual(
                        property,
                        Expression.Constant(
                            null,
                            typeof(string))),
                    Expression.NotEqual(
                        property,
                        Expression.Constant(string.Empty))),

            _ => throw UnsupportedOperator(
                condition.Operator,
                condition.Field)
        };
    }

    /// <summary>Builds a GUID comparison expression for the supplied task query condition.</summary>
    /// <param name="property">The GUID property to compare.</param>
    /// <param name="condition">The task query condition containing the operator and identifier value.</param>
    /// <returns>The resulting GUID comparison expression.</returns>
    private static Expression BuildGuidCondition(
        Expression property,
        TaskQueryConditionDto condition)
    {
        if (!Guid.TryParse(condition.Value, out Guid value))
        {
            throw new BadRequestCustomException(
                "Invalid query value.",
                string.Concat(
                    "'",
                    condition.Value,
                    "' is not a valid identifier for field '",
                    condition.Field,
                    "'."));
        }

        Expression constant = Expression.Constant(
            value,
            typeof(Guid));

        string queryOperator = condition.Operator
            .Trim()
            .ToUpperInvariant();

        return queryOperator switch
        {
            TaskQueryOperators.Equals =>
                Expression.Equal(
                    property,
                    constant),

            TaskQueryOperators.NotEquals =>
                Expression.NotEqual(
                    property,
                    constant),

            _ => throw UnsupportedOperator(
                condition.Operator,
                condition.Field)
        };
    }

    /// <summary>Builds a nullable GUID comparison expression for the supplied task query condition.</summary>
    /// <param name="property">The nullable GUID property to compare.</param>
    /// <param name="condition">The task query condition containing the operator and optional identifier value.</param>
    /// <returns>The resulting nullable GUID comparison expression.</returns>
    private static Expression BuildNullableGuidCondition(
        Expression property,
        TaskQueryConditionDto condition)
    {
        string queryOperator = condition.Operator
            .Trim()
            .ToUpperInvariant();

        if (queryOperator == TaskQueryOperators.IsEmpty)
        {
            return Expression.Equal(
                property,
                Expression.Constant(
                    null,
                    typeof(Guid?)));
        }

        if (queryOperator == TaskQueryOperators.IsNotEmpty)
        {
            return Expression.NotEqual(
                property,
                Expression.Constant(
                    null,
                    typeof(Guid?)));
        }

        if (!Guid.TryParse(condition.Value, out Guid value))
        {
            throw new BadRequestCustomException(
                "Invalid query value.",
                string.Concat(
                    "'",
                    condition.Value,
                    "' is not a valid identifier for field '",
                    condition.Field,
                    "'."));
        }

        Expression constant = Expression.Constant(
            (Guid?)value,
            typeof(Guid?));

        return queryOperator switch
        {
            TaskQueryOperators.Equals =>
                Expression.Equal(
                    property,
                    constant),

            TaskQueryOperators.NotEquals =>
                Expression.NotEqual(
                    property,
                    constant),

            _ => throw UnsupportedOperator(
                condition.Operator,
                condition.Field)
        };
    }

    /// <summary>Creates an expression that invokes a string method with the supplied value.</summary>
    /// <param name="property">The string property on which the method will be invoked.</param>
    /// <param name="methodName">The name of the string method to invoke.</param>
    /// <param name="value">The value to pass to the string method.</param>
    /// <returns>The resulting method-call expression.</returns>
    private static MethodCallExpression CallStringMethod(
        Expression property,
        string methodName,
        Expression value)
    {
        return Expression.Call(
            property,
            typeof(string).GetMethod(
                methodName,
                [typeof(string)])!,
            value);
    }

    /// <summary>Determines whether the supplied logical operator represents an OR operation.</summary>
    /// <param name="logicalOperator">The logical operator to evaluate.</param>
    /// <returns><see langword="true"/> when the operator is OR; otherwise, <see langword="false"/>.</returns>
    private static bool IsOr(string logicalOperator)
    {
        return string.Equals(
            logicalOperator,
            TaskQueryLogicalOperators.Or,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Creates a bad-request exception for an unsupported query operator.</summary>
    /// <param name="queryOperator">The unsupported query operator.</param>
    /// <param name="field">The field for which the operator was supplied.</param>
    /// <returns>The exception describing the unsupported operator.</returns>
    private static BadRequestCustomException UnsupportedOperator(
        string queryOperator,
        string field)
    {
        return new BadRequestCustomException(
            "Invalid query operator.",
            string.Concat(
                "The operator '",
                queryOperator,
                "' is not supported for field '",
                field,
                "'."));
    }

    /// <summary>Builds an assignee comparison using the external user identifier supplied by the frontend.</summary>
    /// <param name="task">The task parameter used by the expression.</param>
    /// <param name="condition">The task query condition containing the operator and assignee user identifier.</param>
    /// <returns>The resulting assignee comparison expression.</returns>
    private static Expression BuildAssigneeUserCondition(
        ParameterExpression task,
        TaskQueryConditionDto condition)
    {
        Expression assigneeBoardAccessId = Expression.Property(
            task,
            nameof(BoardTask.AssigneeBoardAccessId));

        string queryOperator = condition.Operator
            .Trim()
            .ToUpperInvariant();

        if (queryOperator == TaskQueryOperators.IsEmpty)
        {
            return Expression.Equal(
                assigneeBoardAccessId,
                Expression.Constant(
                    null,
                    typeof(Guid?)));
        }

        if (queryOperator == TaskQueryOperators.IsNotEmpty)
        {
            return Expression.NotEqual(
                assigneeBoardAccessId,
                Expression.Constant(
                    null,
                    typeof(Guid?)));
        }

        if (!Guid.TryParse(condition.Value, out Guid value))
        {
            throw new BadRequestCustomException(
                "Invalid query value.",
                string.Concat(
                    "'",
                    condition.Value,
                    "' is not a valid identifier for field '",
                    condition.Field,
                    "'."));
        }

        Expression boardAccess = Expression.Property(
            task,
            nameof(BoardTask.AssigneeBoardAccess));
        Expression userProjectMapping = Expression.Property(
            boardAccess,
            nameof(BoardAccess.UserProjectMapping));
        Expression userId = Expression.Property(
            userProjectMapping,
            nameof(UserProjectMapping.UserId));
        Expression userIdEquals = Expression.Equal(
            userId,
            Expression.Constant(
                value,
                typeof(Guid)));
        Expression hasAssigneeUser = Expression.AndAlso(
            Expression.NotEqual(
                boardAccess,
                Expression.Constant(
                    null,
                    typeof(BoardAccess))),
            Expression.NotEqual(
                userProjectMapping,
                Expression.Constant(
                    null,
                    typeof(UserProjectMapping))));

        return queryOperator switch
        {
            TaskQueryOperators.Equals =>
                Expression.AndAlso(
                    hasAssigneeUser,
                    userIdEquals),

            TaskQueryOperators.NotEquals =>
                Expression.OrElse(
                    Expression.Not(hasAssigneeUser),
                    Expression.Not(userIdEquals)),

            _ => throw UnsupportedOperator(
                condition.Operator,
                condition.Field)
        };
    }
}
