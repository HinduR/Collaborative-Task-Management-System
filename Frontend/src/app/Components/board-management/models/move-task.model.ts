export interface MoveTaskRequest {
  workflowColumnId: string;
}

export interface MoveTaskResponse {
  boardId: string;
  taskId: string;
  previousWorkflowColumnId: string;
  workflowColumnId: string;
  movedByUserId: string;
  movedAt: string;
}
