export interface TaskSummaryResponse {
  id: string;
  workflowColumnId: string;
  title: string;
  description: string | null;
  priorityName: string;
  taskTypeName: string;
  assigneeUserId: string | null;
  assigneeName: string | null;
  isTaskOwner: boolean;
}
