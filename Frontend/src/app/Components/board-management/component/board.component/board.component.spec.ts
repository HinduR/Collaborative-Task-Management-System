import { ComponentFixture, TestBed } from "@angular/core/testing";
import { ActivatedRoute, Router } from "@angular/router";
import { of, throwError } from "rxjs";
import { ToastService } from "src/app/shared/service/toast/toast.service";
import { UserMappingService } from "../../../user-mapping/service/user-mapping-service";
import { BoardTaskService } from "../../services/board-task.service";

import { BoardComponent } from "./board.component";

describe("BoardComponent", () => {
  let component: BoardComponent;
  let fixture: ComponentFixture<BoardComponent>;
  let boardTaskService: jest.Mocked<Partial<BoardTaskService>>;
  let toastService: jest.Mocked<Partial<ToastService>>;
  let router: jest.Mocked<Partial<Router>>;

  const board = {
    id: "board-1",
    boardName: "Sprint Board",
    ownerUserId: "owner-1",
    isBoardOwner: true,
    columns: [
      { id: "todo", columnName: "Todo", sortOrder: 2, roleIds: ["dev"] },
      { id: "doing", columnName: "Doing", sortOrder: 1, roleIds: [] },
    ],
  };

  beforeEach(async () => {
    boardTaskService = {
      getProjectBoards: jest.fn().mockReturnValue(of([board])),
      createBoard: jest.fn(),
      updateBoard: jest.fn(),
      deleteBoard: jest.fn(),
    };
    toastService = {
      success: jest.fn(),
      error: jest.fn(),
    };
    router = { navigate: jest.fn().mockResolvedValue(true) };

    await TestBed.configureTestingModule({
      imports: [BoardComponent],
      providers: [
        { provide: BoardTaskService, useValue: boardTaskService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: UserMappingService, useValue: { getRoles: jest.fn().mockReturnValue(of([])) } },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => (key === "projectId" ? "project-1" : null),
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
  });

  it("should load project boards on init", () => {
    fixture.detectChanges();

    expect(component.projectId).toBe("project-1");
    expect(component.boards).toEqual([board]);
    expect(boardTaskService.getProjectBoards).toHaveBeenCalledWith("project-1");
  });

  it("should show a route error when the project id is missing", async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      imports: [BoardComponent],
      providers: [
        { provide: BoardTaskService, useValue: boardTaskService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: UserMappingService, useValue: { getRoles: jest.fn().mockReturnValue(of([])) } },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => null } } },
        },
      ],
    }).compileComponents();
    const missingFixture = TestBed.createComponent(BoardComponent);
    const missingComponent = missingFixture.componentInstance;

    missingFixture.detectChanges();

    expect(missingComponent.boardListErrorMessage).toBe("The project identifier is missing.");
    expect(boardTaskService.getProjectBoards).not.toHaveBeenCalled();
  });

  it("should validate empty board names", () => {
    component.boardName = "   ";

    component.saveBoard();

    expect(component.boardNameValidationMessage).toBe("Board name is required.");
    expect(boardTaskService.createBoard).not.toHaveBeenCalled();
  });

  it("should create a board and sort it into the list", () => {
    boardTaskService.createBoard!.mockReturnValue(
      of({
        id: "board-2",
        name: "Alpha",
        ownerUserId: "owner-1",
        workflowColumns: [{ id: "todo", name: "Todo", sortOrder: 1 }],
      } as any),
    );
    component.projectId = "project-1";
    component.boards = [board as any];
    component.boardName = " Alpha ";

    component.saveBoard();

    expect(boardTaskService.createBoard).toHaveBeenCalledWith("project-1", { name: "Alpha" });
    expect(component.boards.map((item) => item.boardName)).toEqual(["Alpha", "Sprint Board"]);
    expect(toastService.success).toHaveBeenCalledWith("Board created", '"Alpha" was created successfully.');
  });

  it("should map editable board columns and update a board", () => {
    boardTaskService.updateBoard!.mockReturnValue(
      of({ ...board, boardName: "Updated Board", columns: board.columns } as any),
    );
    component.projectId = "project-1";
    component.boards = [board as any];

    component.onEditBoard(board as any);
    expect(component.dialogColumns.map((column) => column.name)).toEqual(["Todo", "Doing"]);
    component.boardName = " Updated Board ";
    component.saveBoard();

    expect(boardTaskService.updateBoard).toHaveBeenCalledWith("project-1", "board-1", {
      boardName: "Updated Board",
    });
    expect(toastService.success).toHaveBeenCalledWith("Board updated", '"Updated Board" was updated successfully.');
  });

  it("should open and confirm delete for board owners", () => {
    boardTaskService.deleteBoard!.mockReturnValue(of({}));
    component.projectId = "project-1";
    component.boards = [board as any];
    const event = { stopPropagation: jest.fn(), preventDefault: jest.fn() } as any;

    component.openDeleteBoardDialog(event, board as any);
    component.confirmDeleteBoard();

    expect(event.stopPropagation).toHaveBeenCalled();
    expect(boardTaskService.deleteBoard).toHaveBeenCalledWith("project-1", "board-1");
    expect(component.boards).toEqual([]);
    expect(toastService.success).toHaveBeenCalledWith("Board deleted", '"Sprint Board" was deleted successfully.');
  });

  it("should show load errors from the service", () => {
    boardTaskService.getProjectBoards!.mockReturnValueOnce(
      throwError(() => ({ status: 403 })),
    );

    fixture.detectChanges();

    expect(component.boards).toEqual([]);
    expect(component.boardListErrorMessage).toBe("You do not have access to this project.");
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load boards",
      "You do not have access to this project.",
    );
  });

  it("should navigate to board views and back to projects", () => {
    component.projectId = "project-1";

    component.onOpenBoard(board as any);
    component.onBackToProjects();

    expect(router.navigate).toHaveBeenCalledWith(
      ["/projects", "project-1", "boards", "board-1"],
      { state: { board } },
    );
    expect(router.navigate).toHaveBeenCalledWith(["/projects"]);
  });

  it("opens create dialog, loads roles, and clears name errors", () => {
    const userMappingService = TestBed.inject(UserMappingService) as any;
    userMappingService.getRoles.mockReturnValueOnce(
      of([{ label: "Admin", value: "role-1" }]),
    );

    component.boardNameValidationMessage = "Required";
    component.boardSaveErrorMessage = "Failed";

    component.onCreateBoard();
    component.onBoardNameChange();

    expect(component.isBoardDialogVisible).toBe(true);
    expect(component.boardDialogTitle).toBe("Create Board");
    expect(component.boardSaveButtonLabel).toBe("Create Board");
    expect(component.roleOptions).toEqual([{ label: "Admin", value: "role-1" }]);
    expect(component.boardNameValidationMessage).toBe("");
    expect(component.boardSaveErrorMessage).toBe("");
  });

  it("does not edit or delete boards for non-owners", () => {
    const event = { stopPropagation: jest.fn(), preventDefault: jest.fn() } as any;
    const nonOwnerBoard = {
      ...board,
      isBoardOwner: false,
    };

    component.onEditBoard(nonOwnerBoard as any);
    component.openDeleteBoardDialog(event, nonOwnerBoard as any);

    expect(component.isBoardDialogVisible).toBe(false);
    expect(component.isDeleteBoardDialogVisible).toBe(false);
    expect(event.stopPropagation).toHaveBeenCalled();
    expect(event.preventDefault).toHaveBeenCalled();
  });

  it("respects close and delete guards while saving or deleting", () => {
    component.isBoardDialogVisible = true;
    component.isSavingBoard = true;
    component.closeBoardDialog();
    expect(component.isBoardDialogVisible).toBe(true);

    component.isDeleteBoardDialogVisible = true;
    component.boardToDelete = board as any;
    component.isDeletingBoard = true;
    component.closeDeleteBoardDialog();
    component.confirmDeleteBoard();

    expect(component.isDeleteBoardDialogVisible).toBe(true);
    expect(component.boardToDelete).toEqual(board);
    expect(boardTaskService.deleteBoard).not.toHaveBeenCalled();
  });

  it("shows create and update save errors", () => {
    component.projectId = "project-1";
    component.boardName = "Alpha";
    boardTaskService.createBoard!.mockReturnValueOnce(
      throwError(() => ({ status: 409, error: { message: "Duplicate board" } })),
    );

    component.saveBoard();

    expect(component.boardSaveErrorMessage).toBe("Duplicate board");
    expect(toastService.error).toHaveBeenCalledWith("Unable to create board", "Duplicate board");

    component.selectedBoardId = "board-1";
    component.boardName = "Updated";
    boardTaskService.updateBoard!.mockReturnValueOnce(
      throwError(() => ({ status: 403 })),
    );

    component.saveBoard();

    expect(component.boardSaveErrorMessage).toBe("Only the board owner can update this board.");
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to update board",
      "Only the board owner can update this board.",
    );
  });

  it("shows delete errors", () => {
    boardTaskService.deleteBoard!.mockReturnValueOnce(
      throwError(() => ({ status: 404 })),
    );
    component.projectId = "project-1";
    component.boardToDelete = board as any;

    component.confirmDeleteBoard();

    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to delete board",
      "The selected project or board does not exist.",
    );
    expect(component.isDeletingBoard).toBe(false);
  });

  it("shows role loading errors", () => {
    const userMappingService = TestBed.inject(UserMappingService) as any;
    userMappingService.getRoles.mockReturnValueOnce(
      throwError(() => new Error("failed")),
    );

    component.onCreateBoard();

    expect(component.roleOptions).toEqual([]);
    expect(component.isLoadingRoles).toBe(false);
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load roles",
      "Role options could not be loaded. Please try again.",
    );
  });
});
