import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { Observable, map } from "rxjs";

import { environment } from "../../../../environment/environment";

import { DropdownOption } from "../../../shared/models/dropdownOption";
import { ProjectSummary, UserListResponse } from "../model/project-mapping";
import {
  ProjectListRequest,
  UserProjectMapping,
} from "../model/user-project-mapping";
import {
  AssignUserRoleRequest,
  RoleSummary,
  UserRoleMappingResponse,
} from "../model/user-role-mapping";

@Injectable({
  providedIn: "root",
})
export class UserMappingService {
  private readonly httpClient = inject(HttpClient);

  private readonly identityUrl =
    environment.gatewayUrl + environment.endpoints.identity;

  private readonly projectUrl =
    environment.gatewayUrl + environment.endpoints.project;

  getUsers(search?: string): Observable<DropdownOption[]> {
    let params = new HttpParams();

    if (search?.trim()) {
      params = params.set("search", search.trim());
    }

    return this.httpClient
      .get<UserListResponse>(`${this.identityUrl}/users/list`, { params })
      .pipe(
        map((response) =>
          response.items.map((user) => ({
            label: user.displayName,
            value: user.id,
          })),
        ),
      );
  }

  getProjects(): Observable<DropdownOption[]> {
    return this.httpClient
      .get<ProjectSummary[]>(`${this.projectUrl}/admin/projects`)
      .pipe(
        map((projects) =>
          projects.map((project) => ({
            label: project.projectName,
            value: project.id,
          })),
        ),
      );
  }
  getProjectList(): Observable<ProjectSummary[]> {
    return this.httpClient.get<ProjectSummary[]>(
      `${this.projectUrl}/admin/projects`,
    );
  }

  getRoles(): Observable<DropdownOption[]> {
    return this.httpClient.get<RoleSummary[]>(`${this.identityUrl}/role`).pipe(
      map((roles) =>
        roles.map((role) => ({
          label: role.name,
          value: role.id,
        })),
      ),
    );
  }

  getUserProjectMappings(): Observable<UserProjectMapping[]> {
    return this.httpClient.get<UserProjectMapping[]>(
      `${this.projectUrl}/admin/user-project-mappings`,
    );
  }

  saveUserProjectMapping(
    userId: string,
    projectIds: string[],
  ): Observable<void> {
    const request: ProjectListRequest = {
      projectIdList: projectIds,
    };

    return this.httpClient.put<void>(
      `${this.projectUrl}/admin/users/${userId}/projects`,
      request,
    );
  }

  assignUserRole(
    userId: string,
    roleId: string,
  ): Observable<UserRoleMappingResponse> {
    const request: AssignUserRoleRequest = {
      roleId,
    };

    return this.httpClient.put<UserRoleMappingResponse>(
      `${this.identityUrl}/users/${userId}/role`,
      request,
    );
  }
}
