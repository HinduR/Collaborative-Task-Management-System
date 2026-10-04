import { signal } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { ActivatedRoute, Router } from "@angular/router";
import { of, Subject, throwError } from "rxjs";
import { ToastService } from "src/app/shared/service/toast/toast.service";
import { UserMappingService } from "../../../user-mapping/service/user-mapping-service";
import { BoardRealtimeService } from "../../services/board-signalr/board-realtime.service";
import { BoardTaskService } from "../../services/board-task.service";

import { BoardViewComponent } from "./board-view.component";

describe("BoardViewComponent", () => {
  let component: BoardViewComponent;
  let fixture: ComponentFixture<BoardViewComponent>;
  let boardTaskService: jest.Mocked<Partial<BoardTaskService>>;
  let toastService: jest.Mocked<Partial<ToastService>>;
  let router: jest.Mocked<Partial<Router>>;
  let realtimeService: {
    taskMoved$: ReturnType<Subject<any>["asObservable"]>;
    activeUsers: ReturnType<typeof signal<any[]>>;
    connected: ReturnType<typeof signal<boolean>>;
    connect: jest.Mock;
    disconnect: jest.Mock;
  };

  const taskMovedSubject = new Subject<any>();

  beforeEach(async () => {
    window.history.replaceState({}, "");

    boardTaskService = {
      getBoardTasks: jest.fn().mockReturnValue(of([])),
      getProjectUserMappings: jest.fn().mockReturnValue(
        of([
          { id: "mapping-1", userId: "user-1", userName: "Asha" },
          { id: "mapping-2", userId: "user-2", userName: "Zara" },
        ]),
      ),
      getProjectBoards: jest.fn().mockReturnValue(
        of([
          {
            id: "board-1",
            boardName: "Board",
            isBoardOwner: true,
            columns: [
              { id: "done", columnName: "Done", sortOrder: 2, roleIds: ["role-1"] },
              { id: "todo", columnName: "Todo", sortOrder: 1, roleIds: [] },
            ],
          },
        ]),
      ),
      getBoardAccess: jest.fn().mockReturnValue(of([{ userProjectMappingId: "mapping-1" }])),
      replaceBoardAccess: jest.fn().mockReturnValue(of({})),
      getTaskById: jest.fn().mockReturnValue(
        of({
          id: "task-1",
          workflowColumnId: "todo",
          title: "Login",
          priorityName: "High",
          taskTypeName: "Bug",
        } as any),
      ),
      deleteTask: jest.fn().mockReturnValue(of({})),
      moveTask: jest.fn().mockReturnValue(of({})),
      updateWorkflowColumns: jest.fn().mockReturnValue(
        of([
          { id: "done", columnName: "Done", sortOrder: 2, roleIds: ["role-1"] },
          { id: "todo", columnName: "Todo renamed", sortOrder: 1, roleIds: ["role-1"] },
        ]),
      ),
    };
    toastService = {
      success: jest.fn(),
      error: jest.fn(),
      warning: jest.fn(),
    };
    router = { navigate: jest.fn().mockResolvedValue(true) };
    realtimeService = {
      taskMoved$: taskMovedSubject.asObservable(),
      activeUsers: signal([]).asReadonly(),
      connected: signal(false).asReadonly(),
      connect: jest.fn().mockResolvedValue(undefined),
      disconnect: jest.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [BoardViewComponent],
      providers: [
        { provide: BoardTaskService, useValue: boardTaskService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: UserMappingService, useValue: { getRoles: jest.fn().mockReturnValue(of([])) } },
        {
          provide: BoardRealtimeService,
          useValue: realtimeService,
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) =>
                  key === "projectId" ? "project-1" : key === "boardId" ? "board-1" : null,
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BoardViewComponent);
    component = fixture.componentInstance;
    component.projectId = "project-1";
    component.boardId = "board-1";
    component.board = {
      id: "board-1",
      name: "Board",
      isBoardOwner: true,
      workflowColumns: [
        {
          id: "todo",
          name: "Todo",
          sortOrder: 1,
          roleIds: [],
          taskCount: 1,
          tasks: [
            {
              id: "task-1",
              workflowColumnId: "todo",
              title: "Login",
              priority: "High",
              taskType: "Bug",
              isTaskOwner: true,
            },
          ],
        },
        { id: "done", name: "Done", sortOrder: 2, roleIds: [], taskCount: 0, tasks: [] },
      ],
    };
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should expose workflow options, priority classes, and initials", () => {
    expect(component.workflowDropListIds).toEqual(["workflow-column-todo", "workflow-column-done"]);
    expect(component.workflowColumnOptions).toEqual([
      { label: "Todo", value: "todo" },
      { label: "Done", value: "done" },
    ]);
    expect(component.getPriorityClass(" critical ")).toBe("task-priority--critical");
    expect(component.getPriorityClass("unknown")).toBe("task-priority--default");
    expect(component.getAssigneeInitial("Asha Kumar")).toBe("AK");
    expect(component.getAssigneeInitial("")).toBe("?");
  });

  it("should open and save board members for board owners", () => {
    component.openBoardMembersDialog();
    component.boardMembersControl.setValue(["mapping-1", "mapping-2"]);
    component.saveBoardMembers();

    expect(component.isBoardMembersDialogVisible).toBe(false);
    expect(component.boardMemberOptions).toEqual([
      { label: "Asha", value: "mapping-1" },
      { label: "Zara", value: "mapping-2" },
    ]);
    expect(boardTaskService.replaceBoardAccess).toHaveBeenCalledWith("board-1", [
      "mapping-1",
      "mapping-2",
    ]);
    expect(toastService.success).toHaveBeenCalledWith(
      "Members updated",
      "Board access was updated successfully.",
    );
  });

  it("should open task details and delete owned tasks", () => {
    component.onOpenTask("task-1");
    expect(component.isEditTaskPanelVisible).toBe(true);
    expect(component.taskPanelMode).toBe("details");

    const event = { stopPropagation: jest.fn(), preventDefault: jest.fn() } as any;
    component.openDeleteTaskDialog(event, component.board!.workflowColumns[0].tasks[0]);
    component.confirmDeleteTask();

    expect(boardTaskService.deleteTask).toHaveBeenCalledWith("board-1", "task-1");
    expect(component.board!.workflowColumns[0].tasks).toEqual([]);
    expect(toastService.success).toHaveBeenCalledWith("Task deleted", '"Login" was deleted successfully.');
  });

  it("should add and update tasks in the board columns", () => {
    component.onTaskCreated({
      id: "task-2",
      workflowColumnId: "done",
      title: "Deploy",
      description: null,
      priorityName: "Low",
      taskTypeName: "Feature",
      isTaskOwner: true,
    } as any);

    component.onTaskUpdated({
      id: "task-2",
      workflowColumnId: "todo",
      title: "Deploy updated",
      description: null,
      priorityName: "Medium",
      taskTypeName: "Feature",
      isTaskOwner: true,
    } as any);

    expect(component.board!.workflowColumns[0].tasks.map((task) => task.title)).toEqual([
      "Login",
      "Deploy updated",
    ]);
    expect(component.board!.workflowColumns[1].tasks).toEqual([]);
  });

  it("should warn before deleting columns with tasks and navigate to task query", () => {
    component.onDeleteColumn(component.board!.workflowColumns[0]);
    component.openTaskQuery();

    expect(toastService.warning).toHaveBeenCalledWith(
      "Column contains tasks",
      "Move or delete all tasks before deleting this column.",
    );
    expect(router.navigate).toHaveBeenCalledWith([
      "/projects",
      "project-1",
      "boards",
      "board-1",
      "tasks",
    ]);
  });

  it("should show member load errors", () => {
    boardTaskService.getBoardAccess!.mockReturnValueOnce(
      throwError(() => ({ status: 404 })),
    );

    component.openBoardMembersDialog();

    expect(component.boardMemberOptions).toEqual([]);
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load members",
      "The requested board information was not found.",
    );
  });

  it("loads the board from the project when navigation state is not supplied", () => {
    boardTaskService.getBoardTasks!.mockReturnValueOnce(
      of([
        {
          id: "task-2",
          workflowColumnId: "todo",
          title: "Loaded",
          description: "",
          priorityName: "Medium",
          taskTypeName: "Task",
          assigneeUserId: "user-1",
          assigneeName: "Asha",
          isTaskOwner: false,
        },
      ]),
    );

    component.board = null;
    component.ngOnInit();

    expect(boardTaskService.getProjectBoards).toHaveBeenCalledWith("project-1");
    expect(component.board!.workflowColumns.map((column) => column.id)).toEqual(["todo", "done"]);
    expect(component.board!.workflowColumns[0].tasks[0].title).toBe("Loaded");
    expect(component.isLoading).toBe(false);
  });

  it("shows a missing board message when the selected board is not found", () => {
    boardTaskService.getProjectBoards!.mockReturnValueOnce(of([]));

    component.board = null;
    component.ngOnInit();

    expect(component.loadErrorMessage).toBe("The requested board was not found in this project.");
    expect(component.isLoading).toBe(false);
  });

  it("reloads board tasks for realtime events on the current board only", () => {
    component.ngOnInit();
    jest.clearAllMocks();

    taskMovedSubject.next({ boardId: "other-board" });
    taskMovedSubject.next({ boardId: "board-1" });

    expect(boardTaskService.getBoardTasks).toHaveBeenCalledTimes(1);
    expect(boardTaskService.getBoardTasks).toHaveBeenCalledWith("board-1");
  });

  it("moves a task between columns and handles move failures by reloading", () => {
    const sourceColumn = component.board!.workflowColumns[0];
    const targetColumn = component.board!.workflowColumns[1];
    const dropEvent = {
      previousContainer: { data: sourceColumn },
      container: { data: targetColumn },
      previousIndex: 0,
      currentIndex: 0,
      item: { data: sourceColumn.tasks[0] },
    } as any;

    component.onTaskDrop(dropEvent);

    expect(boardTaskService.moveTask).toHaveBeenCalledWith("board-1", "task-1", {
      workflowColumnId: "done",
    });
    expect(toastService.success).toHaveBeenCalledWith("Task moved", '"Login" was moved to Done.');
    expect(component.board!.workflowColumns[0].tasks).toEqual([]);
    expect(component.board!.workflowColumns[1].tasks[0].workflowColumnId).toBe("done");

    boardTaskService.moveTask!.mockReturnValueOnce(throwError(() => ({ status: 403 })));
    component.board!.workflowColumns[0].tasks = [component.board!.workflowColumns[1].tasks[0]];
    component.board!.workflowColumns[1].tasks = [];

    component.onTaskDrop(dropEvent);

    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to move task",
      "You do not have permission to perform this action.",
    );
    expect(boardTaskService.getBoardTasks).toHaveBeenCalled();
  });

  it("saves workflow columns and preserves existing tasks", () => {
    component.saveWorkflowColumns([
      { id: "todo", columnName: " Todo renamed ", roleIds: ["role-1", "role-1"] },
      { id: "done", columnName: "Done", roleIds: ["role-1"] },
    ]);

    expect(boardTaskService.updateWorkflowColumns).toHaveBeenCalledWith("board-1", {
      workflowColumnList: [
        { id: "todo", columnName: "Todo renamed", roleIds: ["role-1"] },
        { id: "done", columnName: "Done", roleIds: ["role-1"] },
      ],
    });
    expect(component.isWorkflowColumnDialogVisible).toBe(false);
    expect(component.board!.workflowColumns[0].name).toBe("Todo renamed");
    expect(component.board!.workflowColumns[0].tasks[0].title).toBe("Login");
  });

  it("loads roles when adding and editing workflow columns", () => {
    const userMappingService = TestBed.inject(UserMappingService) as any;
    userMappingService.getRoles.mockReturnValueOnce(
      of([{ label: "Admin", value: "role-1" }]),
    );

    component.onAddColumn();

    expect(component.isWorkflowColumnDialogVisible).toBe(true);
    expect(component.selectedWorkflowColumnId).toBeNull();
    expect(component.roleOptions).toEqual([{ label: "Admin", value: "role-1" }]);

    component.onEditColumn("todo");

    expect(component.selectedWorkflowColumnId).toBe("todo");
  });

  it("loads task assignees when opening create and edit panels", () => {
    component.taskAssigneeOptions = [];

    component.onAddTask("todo");

    expect(component.selectedWorkflowColumnId).toBe("todo");
    expect(component.isCreateTaskPanelVisible).toBe(true);
    expect(component.taskAssigneeOptions).toEqual([
      { label: "Asha", value: "user-1" },
      { label: "Zara", value: "user-2" },
    ]);

    const event = { stopPropagation: jest.fn(), preventDefault: jest.fn() } as any;
    component.onEditTask(event, "task-1");

    expect(event.stopPropagation).toHaveBeenCalled();
    expect(event.preventDefault).toHaveBeenCalled();
    expect(component.taskPanelMode).toBe("edit");
    expect(component.isEditTaskPanelVisible).toBe(true);
  });

  it("covers close guards and missing identifiers", () => {
    component.isSavingMembers = true;
    component.closeBoardMembersDialog();
    expect(component.isBoardMembersDialogVisible).toBe(false);

    component.isSavingWorkflowColumns = true;
    component.closeWorkflowColumnDialog();
    expect(component.selectedWorkflowColumnId).toBeNull();

    component.projectId = "";
    component.openTaskQuery();
    expect(router.navigate).not.toHaveBeenCalledWith(["/projects", "", "boards", "board-1", "tasks"]);
  });

  it("uses navigation state when it contains the current board", () => {
    window.history.replaceState(
      {
        board: {
          id: "board-1",
          boardName: "State Board",
          isBoardOwner: true,
          columns: [
            { id: "done", columnName: "Done", sortOrder: 2, roleIds: [] },
            { id: "todo", columnName: "Todo", sortOrder: 1, roleIds: [] },
          ],
        },
      },
      "",
    );

    component.board = null;
    component.ngOnInit();

    expect(component.board!.name).toBe("State Board");
    expect(component.board!.workflowColumns.map((column) => column.id)).toEqual(["todo", "done"]);
    expect(boardTaskService.getProjectBoards).not.toHaveBeenCalled();
    expect(boardTaskService.getBoardTasks).toHaveBeenCalledWith("board-1");
  });

  it("shows route and realtime connection errors", async () => {
    TestBed.resetTestingModule();
    realtimeService.connect.mockRejectedValueOnce(new Error("offline"));

    await TestBed.configureTestingModule({
      imports: [BoardViewComponent],
      providers: [
        { provide: BoardTaskService, useValue: boardTaskService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: UserMappingService, useValue: { getRoles: jest.fn().mockReturnValue(of([])) } },
        { provide: BoardRealtimeService, useValue: realtimeService },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => null } } },
        },
      ],
    }).compileComponents();

    const missingFixture = TestBed.createComponent(BoardViewComponent);
    const missingComponent = missingFixture.componentInstance;
    missingFixture.detectChanges();

    expect(missingComponent.loadErrorMessage).toBe("The project or board identifier is missing.");

    component.ngOnInit();
    await Promise.resolve();

    expect(toastService.warning).toHaveBeenCalledWith(
      "Real-time connection unavailable",
      "Changes from other users may require a refresh.",
    );
  });

  it("ignores invalid task drops", () => {
    component.onTaskDrop({} as any);

    const sourceColumn = component.board!.workflowColumns[0];
    const sameContainer = { data: sourceColumn };
    component.onTaskDrop({
      previousContainer: sameContainer,
      container: sameContainer,
      item: { data: sourceColumn.tasks[0] },
    } as any);

    component.movingTaskIds.add("task-1");
    component.onTaskDrop({
      previousContainer: { data: sourceColumn },
      container: { data: component.board!.workflowColumns[1] },
      previousIndex: 0,
      currentIndex: 0,
      item: { data: sourceColumn.tasks[0] },
    } as any);

    expect(boardTaskService.moveTask).not.toHaveBeenCalled();
  });

  it("shows board task load and member save errors", () => {
    boardTaskService.getBoardTasks!.mockReturnValueOnce(
      throwError(() => ({ status: 401 })),
    );

    component.loadBoardTasks();

    expect(component.loadErrorMessage).toBe("Your authentication token is invalid or expired.");
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load board",
      "Your authentication token is invalid or expired.",
    );

    boardTaskService.replaceBoardAccess!.mockReturnValueOnce(
      throwError(() => ({ status: 403 })),
    );

    component.saveBoardMembers();

    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to update members",
      "You do not have permission to perform this action.",
    );
  });

  it("handles workflow-column role and save errors", () => {
    const userMappingService = TestBed.inject(UserMappingService) as any;
    userMappingService.getRoles.mockReturnValueOnce(
      throwError(() => ({ error: { description: "Roles failed" } })),
    );

    component.onAddColumn();

    expect(component.roleOptions).toEqual([]);
    expect(toastService.error).toHaveBeenCalledWith("Unable to load roles", "Roles failed");

    boardTaskService.updateWorkflowColumns!.mockReturnValueOnce(
      throwError(() => ({ status: 0 })),
    );

    component.saveWorkflowColumns([
      { id: "todo", columnName: "Todo", roleIds: [] },
    ]);

    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to save columns",
      "Unable to connect to the BoardTask service.",
    );
  });

  it("handles task-assignee and task-open errors", () => {
    component.taskAssigneeOptions = [];
    boardTaskService.getProjectUserMappings!.mockReturnValueOnce(
      throwError(() => ({ status: 404 })),
    );

    component.onAddTask("todo");

    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load assignees",
      "The requested board information was not found.",
    );

    boardTaskService.getTaskById!.mockReturnValueOnce(
      throwError(() => "bad"),
    );

    component.onOpenTask("task-1");

    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to open task",
      "Unable to load task details.",
    );
  });

  it("handles task update fallback and task delete errors", () => {
    component.selectedWorkflowColumnId = null;
    component.selectedTask = null;
    jest.clearAllMocks();

    component.onTaskUpdated({
      id: "missing",
      workflowColumnId: "",
      title: "Missing",
      description: null,
      priorityName: "Low",
      taskTypeName: "Task",
      isTaskOwner: true,
    } as any);

    expect(boardTaskService.getBoardTasks).toHaveBeenCalledWith("board-1");

    const event = { stopPropagation: jest.fn(), preventDefault: jest.fn() } as any;
    component.openDeleteTaskDialog(event, {
      ...component.board!.workflowColumns[0].tasks[0],
      isTaskOwner: false,
    });

    expect(component.isDeleteTaskDialogVisible).toBe(false);

    boardTaskService.deleteTask!.mockReturnValueOnce(
      throwError(() => ({ error: { message: "Delete failed" } })),
    );
    component.taskToDelete = {
      id: "task-1",
      workflowColumnId: "todo",
      title: "Login",
      priority: "High",
      taskType: "Bug",
      isTaskOwner: true,
    };

    component.confirmDeleteTask();

    expect(toastService.error).toHaveBeenCalledWith("Unable to delete task", "Delete failed");
  });

  it("closes task dialogs and navigates back to boards", () => {
    component.isTaskDetailsVisible = true;
    component.selectedTaskId = "task-1";
    component.closeTaskDetails();

    expect(component.isTaskDetailsVisible).toBe(false);
    expect(component.selectedTaskId).toBeNull();

    component.onBackToBoards();

    expect(router.navigate).toHaveBeenCalledWith(["/projects", "project-1", "boards"]);

    component.isDeleteTaskDialogVisible = true;
    component.taskToDelete = component.board!.workflowColumns[0].tasks[0];
    component.closeDeleteTaskDialog();

    expect(component.isDeleteTaskDialogVisible).toBe(false);
    expect(component.taskToDelete).toBeNull();
  });
});
