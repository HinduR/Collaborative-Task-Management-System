export interface TaskForm {
  title: string;
  description: string;
  priorityRefTermKey: string | null;
  taskTypeRefTermKey: string | null;
  assigneeUserId: string | null;
  workflowColumnId: string | null;
}
