import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environment/environment';
import { RefTermResponse } from '../../../shared/models/reftermrespone';
import { CreateBoardRequest, CreateBoardResponse, UpdateBoardRequest } from '../models/create-board-request';
import { BoardSummaryResponse } from '../models/board-summary-response';
import { CreateTaskRequest } from '../models/create-task-request';
import { TaskSummaryResponse } from '../models/task-summary';
import {
  BoardAccessResponse,
  ProjectUserMappingResponse,
  UpdateBoardAccessRequest,
} from '../models/project-mapping';
import {
  UpdateWorkflowColumnsRequest,
  WorkflowColumnUpdateResponse,
} from '../models/workflow-column-request';
import { MoveTaskRequest, MoveTaskResponse } from '../models/move-task.model';
import { TaskCommentResponse, TaskDetailsResponse, TaskQueryRequest, TaskQueryResponse } from '../models/task-response';
import { TaskCommentRequest } from '../models/comment-request';

@Injectable({
  providedIn: 'root',
})
export class BoardTaskService {
  private readonly httpClient = inject(HttpClient);

  private readonly boardTaskUrl = environment.gatewayUrl + environment.endpoints.boardTask;
  private readonly projectUrl = environment.gatewayUrl + environment.endpoints.project;

  private readonly metadataUrl = environment.gatewayUrl + environment.endpoints.metadata;

  getProjectBoards(projectId: string): Observable<BoardSummaryResponse[]> {
    return this.httpClient.get<BoardSummaryResponse[]>(
      `${this.boardTaskUrl}/projects/${projectId}/boards`,
    );
  }

  createBoard(projectId: string, request: CreateBoardRequest): Observable<CreateBoardResponse> {
    return this.httpClient.post<CreateBoardResponse>(
      `${this.boardTaskUrl}/projects/${projectId}/boards`,
      request,
    );
  }

  updateBoard(
    projectId: string,
    boardId: string,
    request: UpdateBoardRequest,
  ): Observable<BoardSummaryResponse> {
    return this.httpClient.patch<BoardSummaryResponse>(
      `${this.boardTaskUrl}/projects/${projectId}/boards/${boardId}`,
      request,
    );
  }

  deleteBoard(projectId: string, boardId: string): Observable<void> {
    return this.httpClient.delete<void>(
      `${this.boardTaskUrl}/projects/${projectId}/boards/${boardId}`,
      {
        params: {
          confirmDelete: true,
        },
      },
    );
  }

  getBoardTasks(boardId: string): Observable<TaskSummaryResponse[]> {
    return this.httpClient.get<TaskSummaryResponse[]>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks`,
    );
  }

  createTask(boardId: string, request: CreateTaskRequest): Observable<TaskSummaryResponse> {
    return this.httpClient.post<TaskSummaryResponse>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks`,
      request,
    );
  }

  getRefTerms(refSetKey: string): Observable<RefTermResponse[]> {
    return this.httpClient.get<RefTermResponse[]>(
      `${this.metadataUrl}/ref-set/${refSetKey}/ref-terms`,
    );
  }

  getProjectUserMappings(projectId: string): Observable<ProjectUserMappingResponse[]> {
    return this.httpClient.get<ProjectUserMappingResponse[]>(
      `${this.projectUrl}/${projectId}/user-mappings`,
    );
  }

  getBoardAccess(boardId: string): Observable<BoardAccessResponse[]> {
    return this.httpClient.get<BoardAccessResponse[]>(
      `${this.boardTaskUrl}/boards/${boardId}/access`,
    );
  }

  replaceBoardAccess(boardId: string, mappingIds: string[]): Observable<void> {
    const request: UpdateBoardAccessRequest = {
      userProjectMappingIdList: mappingIds,
    };

    return this.httpClient.put<void>(`${this.boardTaskUrl}/boards/${boardId}/access`, request);
  }

  updateWorkflowColumns(
    boardId: string,
    request: UpdateWorkflowColumnsRequest,
  ): Observable<WorkflowColumnUpdateResponse[]> {
    return this.httpClient.put<WorkflowColumnUpdateResponse[]>(
      `${this.boardTaskUrl}/boards/${boardId}/workflow-columns`,
      request,
    );
  }

  moveTask(
    boardId: string,
    taskId: string,
    request: MoveTaskRequest,
  ): Observable<MoveTaskResponse> {
    return this.httpClient.patch<MoveTaskResponse>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}/move`,
      request,
    );
  }
  getTaskById(boardId: string, taskId: string): Observable<TaskDetailsResponse> {
    return this.httpClient.get<TaskDetailsResponse>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}`,
    );
  }

  updateTask(
    boardId: string,
    taskId: string,
    request: CreateTaskRequest,
  ): Observable<TaskSummaryResponse> {
    return this.httpClient.patch<TaskSummaryResponse>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}`,
      request,
    );
  }

  createTaskComment(
    boardId: string,
    taskId: string,
    request: TaskCommentRequest,
  ): Observable<TaskCommentResponse> {
    return this.httpClient.post<TaskCommentResponse>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}/comments`,
      request,
    );
  }

  updateTaskComment(
    boardId: string,
    taskId: string,
    commentId: string,
    request: TaskCommentRequest,
  ): Observable<TaskCommentResponse> {
    return this.httpClient.patch<TaskCommentResponse>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}/comments/${commentId}`,
      request,
    );
  }

  deleteTaskComment(boardId: string, taskId: string, commentId: string): Observable<void> {
    return this.httpClient.delete<void>(
      `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}/comments/${commentId}`,
    );
  }

  queryTasks(request: TaskQueryRequest): Observable<TaskQueryResponse> {
    return this.httpClient.post<TaskQueryResponse>(`${this.boardTaskUrl}/tasks/query`, request);
  }

  deleteTask(
  boardId: string,
  taskId: string,
): Observable<void> {
  return this.httpClient.delete<void>(
    `${this.boardTaskUrl}/boards/${boardId}/tasks/${taskId}`,
  );
}
}
