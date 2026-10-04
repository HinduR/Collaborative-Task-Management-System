export interface TaskQueryCondition {
  logicalOperator: "AND" | "OR";
  field: string;
  operator: string;
  value: string;
}

export interface BoardTaskResponse {
  id: string;
  title: string;
  description: string | null;
  createdBy: string;
  createdByName: string;
  priorityId: string;
  taskTypeId: string;
  workflowColumnId: string;
  assigneeBoardAccessId: string | null;
}

export interface TaskQueryResponse {
  items: BoardTaskResponse[];
  totalCount: number;
}

export interface WorkflowColumnResponse {
  id: string;
  name: string;
}

export interface RefTermResponse {
  id: string;
  refTermKey: string;
}