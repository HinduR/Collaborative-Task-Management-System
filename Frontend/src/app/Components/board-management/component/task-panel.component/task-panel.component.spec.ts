import { ComponentFixture, TestBed } from "@angular/core/testing";
import { of, throwError } from "rxjs";
import { ToastService } from "src/app/shared/service/toast/toast.service";
import { BoardTaskService } from "../../services/board-task.service";

import { TaskPanelComponent } from "./task-panel.component";

describe("TaskPanelComponent", () => {
  let component: TaskPanelComponent;
  let fixture: ComponentFixture<TaskPanelComponent>;
  let boardTaskService: jest.Mocked<Partial<BoardTaskService>>;
  let toastService: jest.Mocked<Partial<ToastService>>;

  beforeEach(async () => {
    boardTaskService = {
      getRefTerms: jest.fn((type: string) =>
        of(
          type === "PRIORITY"
            ? [{ refTermKey: "HIGH", description: "High" }]
            : [{ refTermKey: "BUG", description: "Bug" }],
        ),
      ),
      createTask: jest.fn(),
      updateTask: jest.fn(),
      createTaskComment: jest.fn(),
      updateTaskComment: jest.fn(),
      deleteTaskComment: jest.fn(),
    };
    toastService = {
      success: jest.fn(),
      error: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [TaskPanelComponent],
      providers: [
        { provide: BoardTaskService, useValue: boardTaskService },
        { provide: ToastService, useValue: toastService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskPanelComponent);
    component = fixture.componentInstance;
    component.boardId = "board-1";
    component.selectedWorkflowColumnId = "todo";
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should expose labels for create, edit, and details modes", () => {
    expect(component.panelTitle).toBe("Create Task");
    component.mode = "edit";
    expect(component.panelTitle).toBe("Edit Task");
    component.isSaving = true;
    expect(component.saveButtonLabel).toBe("Saving...");
    component.mode = "details";
    expect(component.panelTitle).toBe("Task Details");
    expect(component.isFormDisabled).toBe(true);
  });

  it("should validate required task fields", () => {
    component.saveTask();
    expect(component.errorMessage).toBe("Task title is required.");

    component.taskForm.title = "x".repeat(201);
    component.saveTask();
    expect(component.errorMessage).toBe("Task title cannot exceed 200 characters.");

    component.taskForm.title = "Fix login";
    component.saveTask();
    expect(component.errorMessage).toBe("Priority is required.");

    component.taskForm.priorityRefTermKey = "HIGH";
    component.saveTask();
    expect(component.errorMessage).toBe("Task type is required.");

    component.taskForm.taskTypeRefTermKey = "BUG";
    component.taskForm.workflowColumnId = null;
    component.saveTask();
    expect(component.errorMessage).toBe("Workflow column is required.");
  });

  it("does not save tasks in details mode and does not cancel while saving", () => {
    const cancelledSpy = jest.spyOn(component.cancelled, "emit");
    component.mode = "details";

    component.saveTask();

    expect(boardTaskService.createTask).not.toHaveBeenCalled();

    component.isSaving = true;
    component.cancel();

    expect(cancelledSpy).not.toHaveBeenCalled();
  });

  it("resets the form and emits cancel when not saving", () => {
    const cancelledSpy = jest.spyOn(component.cancelled, "emit");
    component.taskForm.title = "Draft";

    component.cancel();

    expect(component.taskForm.title).toBe("");
    expect(component.errorMessage).toBe("");
    expect(cancelledSpy).toHaveBeenCalled();
  });

  it("should create a task and emit the created task", () => {
    const createdTask = {
      id: "task-1",
      workflowColumnId: "todo",
      title: "Fix login",
      priorityName: "High",
      taskTypeName: "Bug",
      isTaskOwner: true,
    } as any;
    boardTaskService.createTask!.mockReturnValue(of(createdTask));
    jest.spyOn(component.taskCreated, "emit");
    component.taskForm = {
      title: " Fix login ",
      description: "  OAuth issue  ",
      priorityRefTermKey: "HIGH",
      taskTypeRefTermKey: "BUG",
      assigneeUserId: "user-1",
      workflowColumnId: "todo",
    };

    component.saveTask();

    expect(boardTaskService.createTask).toHaveBeenCalledWith("board-1", {
      title: "Fix login",
      description: "OAuth issue",
      priorityRefTermKey: "HIGH",
      taskTypeRefTermKey: "BUG",
      assigneeUserId: "user-1",
      workflowColumnId: "todo",
    });
    expect(component.taskCreated.emit).toHaveBeenCalledWith(createdTask);
    expect(toastService.success).toHaveBeenCalledWith("Task created", '"Fix login" was added to the board.');
  });

  it("should populate and update an existing task", () => {
    const updatedTask = {
      id: "task-1",
      workflowColumnId: "",
      title: "Updated",
      priorityName: "High",
      taskTypeName: "Bug",
      isTaskOwner: true,
    } as any;
    boardTaskService.updateTask!.mockReturnValue(of(updatedTask));
    jest.spyOn(component.taskUpdated, "emit");
    component.mode = "edit";
    component.task = {
      id: "task-1",
      title: "Old",
      description: "Desc",
      priorityName: "High",
      taskTypeName: "Bug",
      workflowColumnId: "todo",
      comments: [],
    } as any;

    component.ngOnChanges({ task: { currentValue: component.task } } as any);
    component.taskForm.title = "Updated";
    component.saveTask();

    expect(boardTaskService.updateTask).toHaveBeenCalled();
    expect(component.taskUpdated.emit).toHaveBeenCalledWith({
      ...updatedTask,
      workflowColumnId: "todo",
    });
  });

  it("should surface create errors from the API", () => {
    boardTaskService.createTask!.mockReturnValue(
      throwError(() => ({ status: 400, error: { message: "Bad task" } })),
    );
    component.taskForm = {
      title: "Broken",
      description: "",
      priorityRefTermKey: "HIGH",
      taskTypeRefTermKey: "BUG",
      assigneeUserId: null,
      workflowColumnId: "todo",
    };

    component.saveTask();

    expect(component.errorMessage).toBe("Bad task");
    expect(toastService.error).toHaveBeenCalledWith("Unable to create task", "Bad task");
  });

  it("should surface update errors and missing selected tasks", () => {
    component.mode = "edit";
    component.taskForm = {
      title: "Updated",
      description: "",
      priorityRefTermKey: "HIGH",
      taskTypeRefTermKey: "BUG",
      assigneeUserId: null,
      workflowColumnId: "todo",
    };

    component.saveTask();

    expect(component.errorMessage).toBe("The selected task could not be found.");

    component.task = { id: "task-1" } as any;
    boardTaskService.updateTask!.mockReturnValueOnce(
      throwError(() => ({ status: 403 })),
    );

    component.saveTask();

    expect(component.errorMessage).toBe("You do not have permission to update this task.");
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to update task",
      "You do not have permission to update this task.",
    );
  });

  it("loads option fallback labels and shows option load errors", () => {
    boardTaskService.getRefTerms!.mockImplementationOnce(() =>
      of([{ refTermKey: "LOW", description: "" }]),
    );
    boardTaskService.getRefTerms!.mockImplementationOnce(() =>
      of([{ refTermKey: "TASK", description: "" }]),
    );

    component.ngOnInit();

    expect(component.priorityOptions).toEqual([{ label: "LOW", value: "LOW" }]);
    expect(component.taskTypeOptions).toEqual([{ label: "TASK", value: "TASK" }]);

    boardTaskService.getRefTerms!.mockReturnValueOnce(
      throwError(() => ({ status: 0 })),
    );
    boardTaskService.getRefTerms!.mockReturnValueOnce(of([]));

    component.ngOnInit();

    expect(component.priorityOptions).toEqual([]);
    expect(component.taskTypeOptions).toEqual([]);
    expect(component.errorMessage).toBe("Unable to connect to the service.");
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load task options",
      "Unable to connect to the service.",
    );
  });

  it("should add, edit, and delete owned comments", () => {
    component.task = { id: "task-1" } as any;
    const comment = { id: "comment-1", comment: "First", isCommentOwner: true } as any;
    boardTaskService.createTaskComment!.mockReturnValue(of(comment));
    boardTaskService.updateTaskComment!.mockReturnValue(of({ ...comment, comment: "Edited" }));
    boardTaskService.deleteTaskComment!.mockReturnValue(of({}));

    component.newComment = " First ";
    component.addComment();
    component.startEditingComment(component.comments[0]);
    component.editingCommentText = " Edited ";
    component.updateComment(component.comments[0]);
    component.openDeleteCommentDialog(component.comments[0]);
    component.confirmDeleteComment();

    expect(component.comments).toEqual([]);
    expect(component.showDeleteCommentDialog).toBe(false);
    expect(toastService.success).toHaveBeenCalledWith("Comment deleted", "The comment was deleted successfully.");
  });

  it("ignores blank comments and non-owned comment actions", () => {
    const comment = { id: "comment-1", comment: "First", isCommentOwner: false } as any;

    component.addComment();
    component.task = { id: "task-1" } as any;
    component.newComment = "   ";
    component.addComment();
    component.startEditingComment(comment);
    component.updateComment(comment);
    component.openDeleteCommentDialog(comment);

    expect(boardTaskService.createTaskComment).not.toHaveBeenCalled();
    expect(boardTaskService.updateTaskComment).not.toHaveBeenCalled();
    expect(component.editingCommentId).toBeNull();
    expect(component.showDeleteCommentDialog).toBe(false);
  });

  it("handles comment API errors and delete guards", () => {
    const ownedComment = { id: "comment-1", comment: "First", isCommentOwner: true } as any;
    component.task = { id: "task-1" } as any;
    component.comments = [ownedComment];

    boardTaskService.createTaskComment!.mockReturnValueOnce(
      throwError(() => ({ status: 401 })),
    );
    component.newComment = "New";
    component.addComment();
    expect(toastService.error).toHaveBeenCalledWith("Unable to add comment", "Your session has expired.");

    component.startEditingComment(ownedComment);
    component.editingCommentText = "Edited";
    boardTaskService.updateTaskComment!.mockReturnValueOnce(
      throwError(() => ({ status: 400, error: { description: "Bad comment" } })),
    );
    component.updateComment(ownedComment);
    expect(toastService.error).toHaveBeenCalledWith("Unable to update comment", "Bad comment");

    component.openDeleteCommentDialog(ownedComment);
    component.isSavingComment = true;
    component.cancelDeleteComment();
    component.confirmDeleteComment();
    expect(component.showDeleteCommentDialog).toBe(true);

    component.isSavingComment = false;
    boardTaskService.deleteTaskComment!.mockReturnValueOnce(
      throwError(() => ({ status: 403 })),
    );
    component.confirmDeleteComment();
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to delete comment",
      "You do not have permission to create this task.",
    );
  });
});
