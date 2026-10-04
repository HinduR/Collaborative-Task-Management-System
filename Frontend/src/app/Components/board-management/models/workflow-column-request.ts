import { FormControl } from "@angular/forms";

export interface WorkflowColumn {
  id: string;
  name: string;
  sortOrder: number;
  taskCount: number;
  roleIds: string[];
  tasks: WorkflowTask[];
}

export interface WorkflowColumnChange {
  id: string;
  name: string;
  sortOrder: number;
  taskCount: number;
  isNew: boolean;
  roleControl: FormControl<string[]>;
}

export interface WorkflowColumnSaveRequest {
  id: string | null;
  columnName: string;
  roleIds: string[];
}

export interface WorkflowTask {
  id: string;
  workflowColumnId?: string;
  title: string;
  description?: string;
  taskType: string;
  priority: string;
  assigneeUserId?: string | null;
  assigneeName: string | null;
  isTaskOwner?: boolean;
}

export interface UpdateWorkflowColumnsRequest {
  workflowColumnList: WorkflowColumnSaveRequest[];
}

export interface WorkflowColumnUpdateResponse {
  id: string;
  columnName: string;
  sortOrder: number;
  roleIds: string[];
}
