import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { finalize } from "rxjs";

import { ButtonComponent } from "../../../../shared/component/button/button.component";
import { CardComponent } from "../../../../shared/component/card/card.component";
import { DialogComponent } from "../../../../shared/component/dialog/dialog.component";
import { BoardSummaryResponse } from "../../models/board-summary-response";
import { CreateBoardRequest } from "../../models/create-board-request";
import { WorkflowColumn } from "../../models/workflow-column-request";
import { ToastService } from "../../../../shared/service/toast/toast.service";
import { UserMappingService } from "../../../user-mapping/service/user-mapping-service";
import { DropdownOption } from "../../../../shared/models/dropdownOption";
import { BoardTaskService } from "../../services/board-task.service";

@Component({
  selector: "app-board",
  standalone: true,
  imports: [
    FormsModule,
    ButtonComponent,
    CardComponent,
    DialogComponent,
  ],
  templateUrl: "./board.component.html",
  styleUrl: "./board.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  boards: BoardSummaryResponse[] = [];
  isLoadingBoards = false;
  boardListErrorMessage = "";

  isBoardDialogVisible = false;
  isSavingBoard = false;
  isDeleteBoardDialogVisible = false;
  isDeletingBoard = false;
  boardToDelete: BoardSummaryResponse | null = null;

  selectedBoardId: string | null = null;
  boardName = "";
  boardNameValidationMessage = "";
  boardSaveErrorMessage = "";
  dialogColumns: WorkflowColumn[] = [];
  projectId = "";
  roleOptions: DropdownOption[] = [];
  isLoadingRoles = false;

  constructor(
    private readonly router: Router,
    private readonly boardTaskService: BoardTaskService,
    private readonly changeDetectorRef: ChangeDetectorRef,
    private readonly activatedRoute: ActivatedRoute,
    private readonly toastService: ToastService,
    private readonly userMappingService: UserMappingService,
  ) {}

  ngOnInit(): void {
    this.projectId =
      this.activatedRoute.snapshot.paramMap.get("projectId") ?? "";

    if (!this.projectId) {
      this.boardListErrorMessage = "The project identifier is missing.";
      return;
    }

    this.loadBoards();
  }

  get boardDialogTitle(): string {
    return this.selectedBoardId ? "Edit Board" : "Create Board";
  }

  get boardSaveButtonLabel(): string {
    if (this.isSavingBoard) {
      return this.selectedBoardId ? "Saving..." : "Creating...";
    }

    return this.selectedBoardId ? "Save Changes" : "Create Board";
  }

  loadBoards(): void {
    this.isLoadingBoards = true;
    this.boardListErrorMessage = "";

    this.boardTaskService
      .getProjectBoards(this.projectId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoadingBoards = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          this.boards = response;
        },
        error: (error) => {
          this.boards = [];
          this.boardListErrorMessage = this.getBoardListErrorMessage(error);
          this.toastService.error(
            "Unable to load boards",
            this.boardListErrorMessage,
          );
        },
      });
  }

  onCreateBoard(): void {
    this.resetBoardDialog();
    this.isBoardDialogVisible = true;
    this.loadRoles();
  }

  onEditBoard(board: BoardSummaryResponse): void {
    if (!board.isBoardOwner) {
      return;
    }

    this.selectedBoardId = board.id;
    this.boardName = board.boardName;
    this.dialogColumns = board.columns.map((column) => ({
      id: column.id,
      name: column.columnName,
      sortOrder: column.sortOrder,
      taskCount: 0,
      roleIds: column.roleIds ?? [],
      tasks: [],
    }));

    this.clearDialogErrors();
    this.isBoardDialogVisible = true;
    this.loadRoles();
  }

  onOpenBoard(board: BoardSummaryResponse): void {
    void this.router.navigate(
      ["/projects", this.projectId, "boards", board.id],
      {
        state: {
          board,
        },
      },
    );
  }

  onBackToProjects(): void {
    void this.router.navigate(["/projects"]);
  }

  saveBoard(): void {
    const normalizedBoardName = this.boardName.trim();

    this.clearDialogErrors();

    if (!normalizedBoardName) {
      this.boardNameValidationMessage = "Board name is required.";
      return;
    }

    if (this.selectedBoardId) {
      this.updateBoard(this.selectedBoardId, {
        boardName: normalizedBoardName,
      });
      return;
    }

    this.createBoard({
      name: normalizedBoardName,
    });
  }

  onBoardNameChange(): void {
    this.clearDialogErrors();
  }

  closeBoardDialog(): void {
    if (this.isSavingBoard) {
      return;
    }

    this.isBoardDialogVisible = false;
    this.resetBoardDialog();
  }

  openDeleteBoardDialog(event: MouseEvent, board: BoardSummaryResponse): void {
    event.stopPropagation();
    event.preventDefault();

    if (!board.isBoardOwner || this.isDeletingBoard) {
      return;
    }

    this.boardToDelete = board;
    this.isDeleteBoardDialogVisible = true;
    this.changeDetectorRef.markForCheck();
  }

  closeDeleteBoardDialog(): void {
    if (this.isDeletingBoard) {
      return;
    }

    this.isDeleteBoardDialogVisible = false;
    this.boardToDelete = null;
  }

  confirmDeleteBoard(): void {
    const board = this.boardToDelete;

    if (!board || !board.isBoardOwner || this.isDeletingBoard) {
      return;
    }

    this.isDeletingBoard = true;

    this.boardTaskService
      .deleteBoard(this.projectId, board.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isDeletingBoard = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.boards = this.boards.filter((item) => item.id !== board.id);
          this.isDeleteBoardDialogVisible = false;
          this.boardToDelete = null;
          this.toastService.success(
            "Board deleted",
            `"${board.boardName}" was deleted successfully.`,
          );
        },
        error: (error) => {
          this.toastService.error(
            "Unable to delete board",
            this.getBoardDeleteErrorMessage(error),
          );
        },
      });
  }

  private createBoard(request: CreateBoardRequest): void {
    this.isSavingBoard = true;
    this.boardSaveErrorMessage = "";

    this.boardTaskService
      .createBoard(this.projectId, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingBoard = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          const createdBoard: BoardSummaryResponse = {
            id: response.id,
            boardName: response.name,
            ownerUserId: response.ownerUserId,
            isBoardOwner: true,
            columns: response.workflowColumns
              .map((column) => ({
                id: column.id,
                columnName: column.name,
                sortOrder: column.sortOrder,
              }))
              .sort((first, second) => first.sortOrder - second.sortOrder),
          };

          this.boards = [...this.boards, createdBoard].sort((first, second) =>
            first.boardName.localeCompare(second.boardName),
          );

          this.isBoardDialogVisible = false;
          this.resetBoardDialog();
          this.toastService.success(
            "Board created",
            `"${createdBoard.boardName}" was created successfully.`,
          );
        },
        error: (error) => {
          this.boardSaveErrorMessage = this.getBoardSaveErrorMessage(error);
          this.toastService.error(
            "Unable to create board",
            this.boardSaveErrorMessage,
          );
        },
      });
  }

  private updateBoard(boardId: string, request: { boardName: string }): void {
    this.isSavingBoard = true;
    this.boardSaveErrorMessage = "";

    this.boardTaskService
      .updateBoard(this.projectId, boardId, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingBoard = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (updatedBoard) => {
          this.boards = this.boards
            .map((board) =>
              board.id === updatedBoard.id
                ? {
                    ...board,
                    ...updatedBoard,
                    columns: updatedBoard.columns ?? board.columns,
                  }
                : board,
            )
            .sort((first, second) =>
              first.boardName.localeCompare(second.boardName),
            );

          this.isBoardDialogVisible = false;
          this.resetBoardDialog();
          this.toastService.success(
            "Board updated",
            `"${updatedBoard.boardName}" was updated successfully.`,
          );
        },
        error: (error) => {
          this.boardSaveErrorMessage = this.getBoardSaveErrorMessage(error);
          this.toastService.error(
            "Unable to update board",
            this.boardSaveErrorMessage,
          );
        },
      });
  }

  private loadRoles(): void {
    if (this.roleOptions.length > 0 || this.isLoadingRoles) {
      return;
    }

    this.isLoadingRoles = true;

    this.userMappingService
      .getRoles()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoadingRoles = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (roles) => {
          this.roleOptions = roles;
        },
        error: () => {
          this.roleOptions = [];
          this.toastService.error(
            "Unable to load roles",
            "Role options could not be loaded. Please try again.",
          );
        },
      });
  }

  private resetBoardDialog(): void {
    this.selectedBoardId = null;
    this.boardName = "";
    this.dialogColumns = [];
    this.clearDialogErrors();
  }

  private clearDialogErrors(): void {
    this.boardNameValidationMessage = "";
    this.boardSaveErrorMessage = "";
  }

  private getBoardListErrorMessage(error: any): string {
    if (error.status === 0) {
      return "Unable to connect to the BoardTask service.";
    }

    if (error.status === 401) {
      return "Your authentication token is invalid or expired.";
    }

    if (error.status === 403) {
      return "You do not have access to this project.";
    }

    return (
      error.error?.message ??
      error.error?.description ??
      "Unable to load boards."
    );
  }

  private getBoardSaveErrorMessage(error: any): string {
    if (error.status === 0) {
      return "Unable to connect to the BoardTask service.";
    }

    if (error.status === 400) {
      return (
        error.error?.message ??
        error.error?.description ??
        "Please check the board details."
      );
    }

    if (error.status === 401) {
      return "Your authentication token is invalid or expired.";
    }

    if (error.status === 403) {
      return this.selectedBoardId
        ? "Only the board owner can update this board."
        : "You do not have permission to create a board in this project.";
    }

    if (error.status === 409) {
      return error.error?.message ?? "A board with this name already exists.";
    }

    return (
      error.error?.message ??
      error.error?.description ??
      (this.selectedBoardId ? "Unable to update the board." : "Unable to create the board.")
    );
  }

  private getBoardDeleteErrorMessage(error: any): string {
    if (error.status === 0) {
      return "Unable to connect to the BoardTask service.";
    }

    if (error.status === 400) {
      return error.error?.message ?? "Deletion was not confirmed.";
    }

    if (error.status === 401) {
      return "Your authentication token is invalid or expired.";
    }

    if (error.status === 403) {
      return "Only the board owner can delete this board.";
    }

    if (error.status === 404) {
      return "The selected project or board does not exist.";
    }

    return (
      error.error?.message ??
      error.error?.description ??
      "Unable to delete the board."
    );
  }
}
