import { provideHttpClient } from "@angular/common/http";
import {
  HttpTestingController,
  provideHttpClientTesting,
} from "@angular/common/http/testing";
import { TestBed } from "@angular/core/testing";

import { environment } from "../../../../environment/environment";
import { CreateTaskRequest } from "../models/create-task-request";
import { MoveTaskRequest } from "../models/move-task.model";
import { TaskCommentRequest } from "../models/comment-request";
import { BoardTaskService } from "./board-task.service";

describe("BoardTaskService", () => {
  let service: BoardTaskService;
  let httpTestingController: HttpTestingController;

  const boardTaskUrl =
    environment.gatewayUrl + environment.endpoints.boardTask;
  const projectUrl =
    environment.gatewayUrl + environment.endpoints.project;
  const metadataUrl =
    environment.gatewayUrl + environment.endpoints.metadata;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(BoardTaskService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it("should be created", () => {
    expect(service).toBeTruthy();
  });

  it("should get project boards", () => {
    const projectId = "project-1";
    const response = [
      {
        id: "board-1",
        boardName: "Sprint Board",
        ownerUserId: "user-1",
        isBoardOwner: true,
        columns: [],
      },
    ];

    service.getProjectBoards(projectId).subscribe((boards) => {
      expect(boards).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/projects/${projectId}/boards`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("should create a board", () => {
    const projectId = "project-1";
    const requestBody = {
      name: "Sprint Board",
    };
    const response = {
      id: "board-1",
      name: "Sprint Board",
      ownerUserId: "user-1",
      workflowColumns: [],
    };

    service.createBoard(projectId, requestBody).subscribe((createdBoard) => {
      expect(createdBoard).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/projects/${projectId}/boards`,
    );

    expect(request.request.method).toBe("POST");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should update a board", () => {
    const projectId = "project-1";
    const boardId = "board-1";
    const requestBody = {
      boardName: "Updated Board",
    };
    const response = {
      id: boardId,
      boardName: "Updated Board",
      ownerUserId: "user-1",
      isBoardOwner: true,
      columns: [],
    };

    service
      .updateBoard(projectId, boardId, requestBody)
      .subscribe((updatedBoard) => {
        expect(updatedBoard).toEqual(response);
      });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/projects/${projectId}/boards/${boardId}`,
    );

    expect(request.request.method).toBe("PATCH");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should delete a board with confirmation", () => {
    const projectId = "project-1";
    const boardId = "board-1";

    service.deleteBoard(projectId, boardId).subscribe((response) => {
      expect(response).toBeNull();
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/projects/${projectId}/boards/${boardId}?confirmDelete=true`,
    );

    expect(request.request.method).toBe("DELETE");
    expect(request.request.params.get("confirmDelete")).toBe("true");
    request.flush(null);
  });

  it("should get board tasks", () => {
    const boardId = "board-1";
    const response = [
      {
        id: "task-1",
        workflowColumnId: "column-1",
        title: "Write tests",
        description: "Cover service endpoints",
        priorityName: "High",
        taskTypeName: "Task",
        assigneeUserId: "user-1",
        assigneeName: "Harini",
        isTaskOwner: true,
      },
    ];

    service.getBoardTasks(boardId).subscribe((tasks) => {
      expect(tasks).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("should create a task", () => {
    const boardId = "board-1";
    const requestBody: CreateTaskRequest = {
      title: "Write tests",
      description: "Cover service endpoints",
      priorityRefTermKey: "HIGH",
      taskTypeRefTermKey: "TASK",
      assigneeUserId: "user-1",
      workflowColumnId: "column-1",
    };
    const response = {
      id: "task-1",
      workflowColumnId: "column-1",
      title: requestBody.title,
      description: requestBody.description,
      priorityName: "High",
      taskTypeName: "Task",
      assigneeUserId: requestBody.assigneeUserId,
      assigneeName: "Harini",
      isTaskOwner: true,
    };

    service.createTask(boardId, requestBody).subscribe((createdTask) => {
      expect(createdTask).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks`,
    );

    expect(request.request.method).toBe("POST");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should get reference terms", () => {
    const refSetKey = "PRIORITY";
    const response = [
      {
        refTermId: "priority-1",
        refTermKey: "HIGH",
        description: "High",
      },
    ];

    service.getRefTerms(refSetKey).subscribe((refTerms) => {
      expect(refTerms).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${metadataUrl}/ref-set/${refSetKey}/ref-terms`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("should get project user mappings", () => {
    const projectId = "project-1";
    const response = [
      {
        id: "mapping-1",
        userId: "user-1",
        userName: "Harini",
      },
    ];

    service.getProjectUserMappings(projectId).subscribe((mappings) => {
      expect(mappings).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${projectUrl}/${projectId}/user-mappings`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("should get board access", () => {
    const boardId = "board-1";
    const response = [
      {
        userProjectMappingId: "mapping-1",
        userId: "user-1",
        userName: "Harini",
      },
    ];

    service.getBoardAccess(boardId).subscribe((accessList) => {
      expect(accessList).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/access`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("should replace board access", () => {
    const boardId = "board-1";
    const mappingIds = ["mapping-1", "mapping-2"];

    service.replaceBoardAccess(boardId, mappingIds).subscribe((response) => {
      expect(response).toBeNull();
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/access`,
    );

    expect(request.request.method).toBe("PUT");
    expect(request.request.body).toEqual({
      userProjectMappingIdList: mappingIds,
    });
    request.flush(null);
  });

  it("should update workflow columns", () => {
    const boardId = "board-1";
    const requestBody = {
      workflowColumnList: [
        {
          id: "column-1",
          columnName: "To Do",
          roleIds: ["role-1"],
        },
      ],
    };
    const response = [
      {
        id: "column-1",
        columnName: "To Do",
        sortOrder: 1,
        roleIds: ["role-1"],
      },
    ];

    service
      .updateWorkflowColumns(boardId, requestBody)
      .subscribe((updatedColumns) => {
        expect(updatedColumns).toEqual(response);
      });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/workflow-columns`,
    );

    expect(request.request.method).toBe("PUT");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should move a task", () => {
    const boardId = "board-1";
    const taskId = "task-1";
    const requestBody: MoveTaskRequest = {
      workflowColumnId: "column-2",
    };
    const response = {
      boardId,
      taskId,
      previousWorkflowColumnId: "column-1",
      workflowColumnId: requestBody.workflowColumnId,
      movedByUserId: "user-1",
      movedAt: "2026-08-03T09:00:00.000Z",
    };

    service.moveTask(boardId, taskId, requestBody).subscribe((moveResult) => {
      expect(moveResult).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}/move`,
    );

    expect(request.request.method).toBe("PATCH");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should get task by id", () => {
    const boardId = "board-1";
    const taskId = "task-1";
    const response = {
      id: taskId,
      title: "Write tests",
      description: "Cover service endpoints",
      priorityName: "High",
      taskTypeName: "Task",
      priorityRefTermKey: "HIGH",
      taskTypeRefTermKey: "TASK",
      assigneeUserId: "user-1",
      assigneeName: "Harini",
      workflowColumnId: "column-1",
      isTaskOwner: true,
      comments: [],
    };

    service.getTaskById(boardId, taskId).subscribe((task) => {
      expect(task).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}`,
    );

    expect(request.request.method).toBe("GET");
    request.flush(response);
  });

  it("should update a task", () => {
    const boardId = "board-1";
    const taskId = "task-1";
    const requestBody: CreateTaskRequest = {
      title: "Updated task",
      description: "Updated description",
      priorityRefTermKey: "MEDIUM",
      taskTypeRefTermKey: "BUG",
      assigneeUserId: null,
      workflowColumnId: "column-2",
    };
    const response = {
      id: taskId,
      workflowColumnId: requestBody.workflowColumnId,
      title: requestBody.title,
      description: requestBody.description,
      priorityName: "Medium",
      taskTypeName: "Bug",
      assigneeUserId: null,
      assigneeName: null,
      isTaskOwner: true,
    };

    service.updateTask(boardId, taskId, requestBody).subscribe((task) => {
      expect(task).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}`,
    );

    expect(request.request.method).toBe("PATCH");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should create a task comment", () => {
    const boardId = "board-1";
    const taskId = "task-1";
    const requestBody: TaskCommentRequest = {
      comment: "Looks good",
    };
    const response = {
      id: "comment-1",
      comment: requestBody.comment,
      createdByName: "Harini",
      isCommentOwner: true,
    };

    service
      .createTaskComment(boardId, taskId, requestBody)
      .subscribe((comment) => {
        expect(comment).toEqual(response);
      });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}/comments`,
    );

    expect(request.request.method).toBe("POST");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should update a task comment", () => {
    const boardId = "board-1";
    const taskId = "task-1";
    const commentId = "comment-1";
    const requestBody: TaskCommentRequest = {
      comment: "Updated comment",
    };
    const response = {
      id: commentId,
      comment: requestBody.comment,
      createdByName: "Harini",
      isCommentOwner: true,
    };

    service
      .updateTaskComment(boardId, taskId, commentId, requestBody)
      .subscribe((comment) => {
        expect(comment).toEqual(response);
      });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}/comments/${commentId}`,
    );

    expect(request.request.method).toBe("PATCH");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should delete a task comment", () => {
    const boardId = "board-1";
    const taskId = "task-1";
    const commentId = "comment-1";

    service
      .deleteTaskComment(boardId, taskId, commentId)
      .subscribe((response) => {
        expect(response).toBeNull();
      });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}/comments/${commentId}`,
    );

    expect(request.request.method).toBe("DELETE");
    request.flush(null);
  });

  it("should query tasks", () => {
    const requestBody = {
      conditions: [
        {
          logicalOperator: "AND" as const,
          field: "TASK_TITLE",
          operator: "EQUALS",
          value: "Write tests",
        },
      ],
      pageNumber: 1,
      pageSize: 20,
    };
    const response = {
      items: [],
      totalCount: 0,
    };

    service.queryTasks(requestBody).subscribe((queryResponse) => {
      expect(queryResponse).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/tasks/query`,
    );

    expect(request.request.method).toBe("POST");
    expect(request.request.body).toEqual(requestBody);
    request.flush(response);
  });

  it("should delete a task", () => {
    const boardId = "board-1";
    const taskId = "task-1";

    service.deleteTask(boardId, taskId).subscribe((response) => {
      expect(response).toBeNull();
    });

    const request = httpTestingController.expectOne(
      `${boardTaskUrl}/boards/${boardId}/tasks/${taskId}`,
    );

    expect(request.request.method).toBe("DELETE");
    request.flush(null);
  });
});
