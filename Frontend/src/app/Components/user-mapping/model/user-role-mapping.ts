export interface RoleSummary {
  id: string;
  name: string;
}

export interface AssignUserRoleRequest {
  roleId: string;
}

export interface UserRoleMappingResponse {
  mappingId: string;
  userId: string;
  userName: string;
  roleId: string;
  roleName: string;
}
