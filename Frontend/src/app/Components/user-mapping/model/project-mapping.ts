export interface UserListItem {
  id: string;
  email: string;
  displayName: string;
}

export interface UserListResponse {
  items: UserListItem[];
}

export interface ProjectSummary {
  id: string;
  projectName: string;
  isAssigned: boolean;
}

export interface SaveUserProjectMappingRequest {
  projectIds: string[];
}
