namespace BoardTaskService.Application.TaskModule.Common.Constants;

public static class TaskQueryFields
{
    public const string TaskTitle = "TASK_TITLE";
    public const string Priority = "PRIORITY";
    public const string TaskType = "TASK_TYPE";
    public const string AssignedTo = "ASSIGNEE";
    public const string WorkflowColumn = "WORKFLOW_COLUMN";
}

public static class TaskQueryOperators
{
    public const string Equals = "EQUALS";
    public const string NotEquals = "NOT_EQUALS";
    public const string Contains = "CONTAINS";
    public const string NotContains = "NOT_CONTAINS";
    public const string StartsWith = "STARTS_WITH";
    public const string EndsWith = "ENDS_WITH";
    public const string IsEmpty = "IS_EMPTY";
    public const string IsNotEmpty = "IS_NOT_EMPTY";
}

public static class TaskQueryLogicalOperators
{
    public const string And = "AND";
    public const string Or = "OR";
}