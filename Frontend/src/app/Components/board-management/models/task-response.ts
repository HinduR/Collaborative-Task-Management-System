export interface TaskDetailsResponse {
  id: string;
  title: string;
  description: string | null;
  priorityName: string;
  taskTypeName: string;
  priorityRefTermKey: string;
  taskTypeRefTermKey: string;
  assigneeUserId: string | null;
  assigneeName: string | null;
  workflowColumnId: string;
  isTaskOwner: boolean;
  comments: TaskCommentResponse[];
}

export interface TaskCommentResponse {
  id: string;
  comment: string;
  createdByName: string;
  isCommentOwner: boolean;
}

export interface TaskQueryRequest {
  conditions: TaskQueryCondition[];
  pageNumber: number;
  pageSize: number;
}

export interface TaskQueryResponse {
  items: TaskDetailsResponse[];
  totalCount: number;
}

export interface TaskQueryCondition {
  logicalOperator: 'AND' | 'OR';
  field: string;
  operator: string;
  value: string;
}