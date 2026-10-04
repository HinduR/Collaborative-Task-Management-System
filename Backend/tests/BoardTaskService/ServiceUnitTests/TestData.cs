using BoardTaskService.Application.TaskModule.Dto;

namespace BoardTaskService.ServiceUnitTests;

internal static class TestData
{
    public static TaskRequestDto TaskRequest(Guid? workflowColumnId = null)
    {
        return new TaskRequestDto
        {
            Title = "Implement board view",
            Description = "Build the board task workflow.",
            PriorityRefTermKey = "HIGH",
            TaskTypeRefTermKey = "STORY",
            WorkflowColumnId = workflowColumnId ?? Guid.NewGuid()
        };
    }
}
