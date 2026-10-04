export interface ProjectUserMappingResponse {
  id: string;
  userId: string;
  userName: string;
}

export interface BoardAccessResponse {
  userProjectMappingId: string;
}

export interface UpdateBoardAccessRequest {
  userProjectMappingIdList: string[];
}
