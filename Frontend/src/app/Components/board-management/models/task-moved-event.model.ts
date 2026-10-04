export interface TaskMovedEvent {
  boardId: string;
  taskId: string;
  previousWorkflowColumnId: string;
  workflowColumnId: string;
  movedByUserId: string;
  movedAt: string;
}
