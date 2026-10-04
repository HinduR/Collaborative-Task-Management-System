export interface CreateBoardRequest {
  name: string;
}

export interface UpdateBoardRequest {
  boardName: string;
}

export interface CreateWorkflowColumnRequest {
  workflowColumnName: string;
  roleIds: string[];
}

export interface CreateBoardResponse {
  id: string;
  name: string;
  ownerUserId: string;
  workflowColumns: WorkflowColumnResponse[];
}

export interface WorkflowColumnResponse {
  id: string;
  name: string;
  sortOrder: number;
}
