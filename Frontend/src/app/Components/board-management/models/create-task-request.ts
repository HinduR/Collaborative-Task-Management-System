export interface CreateTaskRequest {
  title: string;
  description: string | null;
  priorityRefTermKey: string;
  taskTypeRefTermKey: string;
  assigneeUserId: string | null;
  workflowColumnId: string;
}
