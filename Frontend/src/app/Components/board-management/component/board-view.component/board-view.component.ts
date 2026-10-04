import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, first, forkJoin, switchMap, tap } from 'rxjs';

import { ButtonComponent } from '../../../../shared/component/button/button.component';
import { CardComponent } from '../../../../shared/component/card/card.component';
import { DialogComponent } from '../../../../shared/component/dialog/dialog.component';
import { MultiSelectComponent } from '../../../../shared/component/multi-select/multi-select.component';

import { CdkDragDrop, DragDropModule, transferArrayItem } from '@angular/cdk/drag-drop';
import { DropdownOption } from '../../../../shared/models/dropdownOption';
import { ToastService } from '../../../../shared/service/toast/toast.service';
import { UserMappingService } from '../../../user-mapping/service/user-mapping-service';
import { BoardSummaryResponse } from '../../models/board-summary-response';
import { BoardNavigationState, BoardView } from '../../models/board-view.model';
import { TaskDetailsResponse } from '../../models/task-response';
import { TaskSummaryResponse } from '../../models/task-summary';
import {
  UpdateWorkflowColumnsRequest,
  WorkflowColumn,
  WorkflowColumnSaveRequest,
  WorkflowColumnUpdateResponse,
  WorkflowTask,
} from '../../models/workflow-column-request';
import { BoardRealtimeService } from '../../services/board-signalr/board-realtime.service';
import { BoardTaskService } from '../../services/board-task.service';
import { TaskPanelComponent } from '../task-panel.component/task-panel.component';
import { UserPresenceComponent } from '../user-presence.component/user-presence.component';
import { WorkflowColumnComponent } from '../workflow-column.component/workflow-column.component';

@Component({
  selector: 'app-board-view',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    CardComponent,
    DialogComponent,
    MultiSelectComponent,
    WorkflowColumnComponent,
    TaskPanelComponent,
    DragDropModule,
    UserPresenceComponent,
  ],
  templateUrl: './board-view.component.html',
  styleUrl: './board-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardViewComponent implements OnInit, OnDestroy {
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly userMappingService = inject(UserMappingService);
  private readonly boardRealtimeService = inject(BoardRealtimeService);

  private readonly router = inject(Router);

  private readonly boardTaskService = inject(BoardTaskService);

  private readonly changeDetectorRef = inject(ChangeDetectorRef);

  private readonly destroyRef = inject(DestroyRef);
  private readonly toastService = inject(ToastService);

  readonly boardMembersControl = new FormControl<string[]>([], {
    nonNullable: true,
  });
  roleOptions: DropdownOption[] = [];
  readonly movingTaskIds = new Set<string>();
  isLoadingRoles = false;
  isSavingWorkflowColumns = false;
  projectId = '';
  boardId = '';
  selectedTask: TaskDetailsResponse | null = null;
  isEditTaskPanelVisible = false;
  taskPanelMode: 'edit' | 'details' = 'details';
  loadingTaskId: string | null = null;
  isDeletingTask = false;
  isDeleteTaskDialogVisible = false;
  taskToDelete: WorkflowTask | null = null;

  board: BoardView | null = null;

  isLoading = false;
  loadErrorMessage = '';
  actionErrorMessage = '';

  readonly maximumColumnCount = 5;

  // Workflow-column dialog
  isWorkflowColumnDialogVisible = false;
  selectedWorkflowColumnId: string | null = null;

  // Task panel
  isCreateTaskPanelVisible = false;

  // Task details
  isTaskDetailsVisible = false;
  selectedTaskId: string | null = null;

  // Board members
  isBoardMembersDialogVisible = false;
  isLoadingMembers = false;
  isSavingMembers = false;
  readonly activeUsers = this.boardRealtimeService.activeUsers;

  readonly realtimeConnected = this.boardRealtimeService.connected;

  boardMemberOptions: DropdownOption[] = [];
  taskAssigneeOptions: DropdownOption[] = [];

  ngOnInit(): void {
    this.projectId = this.activatedRoute.snapshot.paramMap.get('projectId') ?? '';

    this.boardId = this.activatedRoute.snapshot.paramMap.get('boardId') ?? '';

    if (!this.projectId || !this.boardId) {
      this.loadErrorMessage = 'The project or board identifier is missing.';
      return;
    }

    this.listenForTaskMovement();

    void this.connectToBoardHub();

    const navigationState = window.history.state as BoardNavigationState;

    const selectedBoard = navigationState.board;

    if (selectedBoard?.id === this.boardId) {
      this.board = this.createBoardView(selectedBoard);
      this.loadBoardTasks();
      return;
    }

    this.loadBoardFromProject();
  }

  ngOnDestroy(): void {
    void this.disconnectFromBoardHub();
  }

  private async disconnectFromBoardHub(): Promise<void> {
    try {
      await this.boardRealtimeService.disconnect();
    } catch {
      // Ignore cleanup errors during component destruction.
    }
  }

  private async connectToBoardHub(): Promise<void> {
    try {
      await this.boardRealtimeService.connect(this.boardId);
    } catch {
      this.toastService.warning(
        'Real-time connection unavailable',
        'Changes from other users may require a refresh.',
      );
    }
  }

  private listenForTaskMovement(): void {
    this.boardRealtimeService.taskMoved$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event) => {
        if (event.boardId !== this.boardId) {
          return;
        }

        this.loadBoardTasks(false);
      });
  }

  get workflowDropListIds(): string[] {
    return this.board?.workflowColumns.map((column) => `workflow-column-${column.id}`) ?? [];
  }
  onTaskDrop(event: CdkDragDrop<WorkflowColumn>): void {
    if (!this.board || !this.boardId) {
      return;
    }

    // The current backend cannot save ordering within one column.
    if (event.previousContainer === event.container) {
      return;
    }

    const task = event.item.data as WorkflowTask;
    const targetColumn = event.container.data;

    if (!task || !targetColumn || this.movingTaskIds.has(task.id)) {
      return;
    }

    this.actionErrorMessage = '';
    this.movingTaskIds.add(task.id);

    // Optimistically update the UI.
    transferArrayItem(
      event.previousContainer.data.tasks,
      event.container.data.tasks,
      event.previousIndex,
      event.currentIndex,
    );

    task.workflowColumnId = targetColumn.id;

    this.refreshWorkflowColumns();

    this.boardTaskService
      .moveTask(this.boardId, task.id, {
        workflowColumnId: targetColumn.id,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.movingTaskIds.delete(task.id);
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.toastService.success(
            'Task moved',
            `"${task.title}" was moved to ${targetColumn.name}.`,
          );
        },
        error: (error) => {
          const message = this.getErrorMessage(error, 'Unable to move the task.');
          this.toastService.error('Unable to move task', message);
          this.loadBoardTasks();
        },
      });
  }

  private refreshWorkflowColumns(): void {
    if (!this.board) {
      return;
    }

    this.board = {
      ...this.board,
      workflowColumns: this.board.workflowColumns.map((column) => ({
        ...column,
        tasks: [...column.tasks],
        taskCount: column.tasks.length,
      })),
    };

    this.changeDetectorRef.markForCheck();
  }

  private loadBoardFromProject(): void {
    this.isLoading = true;
    this.loadErrorMessage = '';

    this.boardTaskService
      .getProjectBoards(this.projectId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap((boards) => {
          const selectedBoard = boards.find((board) => board.id === this.boardId);

          if (!selectedBoard) {
            throw new Error('BOARD_NOT_FOUND');
          }

          this.board = this.createBoardView(selectedBoard);
        }),
        switchMap(() => this.boardTaskService.getBoardTasks(this.boardId)),
        finalize(() => {
          this.isLoading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (tasks) => {
          this.attachTasksToColumns(tasks ?? []);
        },
        error: (error) => {
          this.loadErrorMessage =
            error instanceof Error && error.message === 'BOARD_NOT_FOUND'
              ? 'The requested board was not found in this project.'
              : this.getErrorMessage(error, 'Unable to load the board.');
        },
      });
  }

  get workflowColumnOptions(): DropdownOption[] {
    return (
      this.board?.workflowColumns.map((column) => ({
        label: column.name,
        value: column.id,
      })) ?? []
    );
  }

  loadBoardTasks(showLoading = true): void {
    if (!this.boardId || !this.board) {
      return;
    }

    if (showLoading) {
      this.isLoading = true;
    }

    this.loadErrorMessage = '';

    this.boardTaskService
      .getBoardTasks(this.boardId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          if (showLoading) {
            this.isLoading = false;
          }

          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (tasks) => {
          this.attachTasksToColumns(tasks ?? []);
        },
        error: (error) => {
          this.loadErrorMessage = this.getErrorMessage(error, 'Unable to load board tasks.');
          this.toastService.error('Unable to load board', this.loadErrorMessage);
        },
      });
  }

  private createBoardView(response: BoardSummaryResponse): BoardView {
    return {
      id: response.id,
      name: response.boardName,
      isBoardOwner: response.isBoardOwner,
      workflowColumns: [...response.columns]
        .sort((first, second) => first.sortOrder - second.sortOrder)
        .map((column) => ({
          id: column.id,
          name: column.columnName,
          sortOrder: column.sortOrder,
          roleIds: column.roleIds ?? [],
          taskCount: 0,
          tasks: [],
        })),
    };
  }

  private attachTasksToColumns(responses: TaskSummaryResponse[]): void {
    if (!this.board) {
      return;
    }

    const tasksByColumnId = new Map<string, WorkflowTask[]>();

    for (const response of responses) {
      const columnTasks = tasksByColumnId.get(response.workflowColumnId) ?? [];

      columnTasks.push(this.mapTask(response));

      tasksByColumnId.set(response.workflowColumnId, columnTasks);
    }

    this.board = {
      ...this.board,
      workflowColumns: this.board.workflowColumns.map((column) => {
        const tasks = tasksByColumnId.get(column.id) ?? [];

        return {
          ...column,
          tasks,
          taskCount: tasks.length,
        };
      }),
    };

    this.changeDetectorRef.markForCheck();
  }

  private mapTask(response: TaskSummaryResponse): WorkflowTask {
    return {
      id: response.id,
      workflowColumnId: response.workflowColumnId,
      title: response.title,
      description: response.description || undefined,
      priority: response.priorityName,
      taskType: response.taskTypeName,
      assigneeUserId: response.assigneeUserId,
      assigneeName: response.assigneeName,
      isTaskOwner: response.isTaskOwner,
    };
  }

  // Board-member access

  openBoardMembersDialog(): void {
    if (!this.board?.isBoardOwner) {
      return;
    }

    if (!this.projectId) {
      this.actionErrorMessage = 'The project identifier is unavailable.';
      return;
    }

    this.actionErrorMessage = '';
    this.isBoardMembersDialogVisible = true;
    this.loadBoardMembers();
  }

  closeBoardMembersDialog(): void {
    if (this.isSavingMembers) {
      return;
    }

    this.isBoardMembersDialogVisible = false;
    this.actionErrorMessage = '';
  }

  private loadBoardMembers(): void {
    if (!this.projectId || !this.boardId) {
      this.actionErrorMessage = 'The project or board identifier is unavailable.';
      return;
    }

    this.isLoadingMembers = true;
    this.actionErrorMessage = '';
    this.boardMembersControl.disable({
      emitEvent: false,
    });

    forkJoin({
      projectMappings: this.boardTaskService.getProjectUserMappings(this.projectId),
      boardAccess: this.boardTaskService.getBoardAccess(this.boardId),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoadingMembers = false;

          if (!this.isSavingMembers) {
            this.boardMembersControl.enable({
              emitEvent: false,
            });
          }

          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: ({ projectMappings, boardAccess }) => {
          this.boardMemberOptions = projectMappings.map((mapping) => ({
            label: mapping.userName,
            value: mapping.id,
          }));
          this.taskAssigneeOptions = projectMappings.map((mapping) => ({
            label: mapping.userName,
            value: mapping.userId,
          }));

          this.boardMembersControl.setValue(
            boardAccess.map((access) => access.userProjectMappingId),
            {
              emitEvent: false,
            },
          );
        },
        error: (error) => {
          this.boardMemberOptions = [];

          this.boardMembersControl.setValue([], {
            emitEvent: false,
          });

          const message = this.getErrorMessage(error, 'Unable to load board members.');
          this.toastService.error('Unable to load members', message);
        },
      });
  }

  saveBoardMembers(): void {
    if (!this.board?.isBoardOwner || !this.boardId || this.isSavingMembers) {
      return;
    }

    this.isSavingMembers = true;
    this.actionErrorMessage = '';

    const selectedMappingIds = this.boardMembersControl.getRawValue();

    this.boardMembersControl.disable({
      emitEvent: false,
    });

    this.boardTaskService
      .replaceBoardAccess(this.boardId, selectedMappingIds)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingMembers = false;
          this.boardMembersControl.enable({
            emitEvent: false,
          });
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.isBoardMembersDialogVisible = false;
          this.actionErrorMessage = '';
          this.toastService.success('Members updated', 'Board access was updated successfully.');
        },
        error: (error) => {
          const message = this.getErrorMessage(error, 'Unable to update board access.');
          this.toastService.error('Unable to update members', message);
        },
      });
  }

  // Workflow columns
  onAddColumn(): void {
    if (!this.board?.isBoardOwner) {
      return;
    }

    this.selectedWorkflowColumnId = null;
    this.openWorkflowColumnDialog();
  }
  private openWorkflowColumnDialog(): void {
    this.actionErrorMessage = '';
    this.isWorkflowColumnDialogVisible = true;

    if (this.roleOptions.length === 0) {
      this.loadRoles();
    }
  }

  private loadRoles(): void {
    if (this.isLoadingRoles) {
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
        error: (error) => {
          this.roleOptions = [];
          const message = this.getErrorMessage(error, 'Unable to load roles.');
          this.toastService.error('Unable to load roles', message);
        },
      });
  }

  onEditColumn(columnId: string): void {
    if (!this.board?.isBoardOwner) {
      return;
    }

    this.selectedWorkflowColumnId = columnId;
    this.openWorkflowColumnDialog();
  }

  closeWorkflowColumnDialog(): void {
    if (this.isSavingWorkflowColumns) {
      return;
    }

    this.isWorkflowColumnDialogVisible = false;
    this.selectedWorkflowColumnId = null;
    this.actionErrorMessage = '';
  }
  saveWorkflowColumns(columns: WorkflowColumnSaveRequest[]): void {
    if (!this.boardId || !this.board?.isBoardOwner || this.isSavingWorkflowColumns) {
      return;
    }

    const request: UpdateWorkflowColumnsRequest = {
      workflowColumnList: columns.map(
        (column): WorkflowColumnSaveRequest => ({
          id: column.id,
          columnName: column.columnName.trim(),
          roleIds: [...new Set(column.roleIds)],
        }),
      ),
    };

    this.isSavingWorkflowColumns = true;
    this.actionErrorMessage = '';

    this.boardTaskService
      .updateWorkflowColumns(this.boardId, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingWorkflowColumns = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          this.applyUpdatedWorkflowColumns(response);

          // Close directly because isSavingWorkflowColumns is still true here.
          this.isWorkflowColumnDialogVisible = false;

          this.toastService.success('Columns updated', 'Workflow columns were saved successfully.');

          this.changeDetectorRef.markForCheck();
        },
        error: (error) => {
          const message = this.getErrorMessage(error, 'Unable to save workflow columns.');

          this.toastService.error('Unable to save columns', message);
        },
      });
  }
  private applyUpdatedWorkflowColumns(response: WorkflowColumnUpdateResponse[]): void {
    if (!this.board) {
      return;
    }

    const existingColumns = new Map(
      this.board.workflowColumns.map((column) => [column.id, column]),
    );

    this.board = {
      ...this.board,
      workflowColumns: [...response]
        .sort((first, second) => first.sortOrder - second.sortOrder)
        .map((column) => {
          const existingColumn = existingColumns.get(column.id);

          const tasks = existingColumn?.tasks ?? [];

          return {
            id: column.id,
            name: column.columnName,
            sortOrder: column.sortOrder,
            roleIds: column.roleIds ?? [],
            tasks,
            taskCount: tasks.length,
          };
        }),
    };
  }

  onDeleteColumn(column: WorkflowColumn): void {
    if (!this.board?.isBoardOwner) {
      return;
    }

    if (column.tasks.length > 0) {
      this.toastService.warning(
        'Column contains tasks',
        'Move or delete all tasks before deleting this column.',
      );
      return;
    }
    this.openWorkflowColumnDialog();
  }

  // Tasks

  onAddTask(workflowColumnId: string): void {
    this.selectedWorkflowColumnId = workflowColumnId;
    this.loadTaskAssignees();
    this.isCreateTaskPanelVisible = true;
  }

  private loadTaskAssignees(): void {
    if (this.taskAssigneeOptions.length > 0 || !this.projectId) {
      return;
    }

    this.boardTaskService
      .getProjectUserMappings(this.projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (mappings) => {
          this.taskAssigneeOptions = mappings.map((mapping) => ({
            label: mapping.userName,
            value: mapping.userId,
          }));
          this.changeDetectorRef.markForCheck();
        },
        error: (error) => {
          const message = this.getErrorMessage(error, 'Unable to load assignees.');
          this.toastService.error('Unable to load assignees', message);
        },
      });
  }

  closeCreateTaskPanel(): void {
    this.isCreateTaskPanelVisible = false;
    this.selectedWorkflowColumnId = null;
  }

  onTaskCreated(createdTask: TaskSummaryResponse): void {
    this.addCreatedTask(createdTask);
    this.closeCreateTaskPanel();
  }

  onTaskUpdated(updatedTask: TaskSummaryResponse): void {
    this.applyUpdatedTask(updatedTask);
    this.closeEditTaskPanel();
  }

  private addCreatedTask(createdTask: TaskSummaryResponse): void {
    if (!this.board) {
      return;
    }

    this.board = {
      ...this.board,
      workflowColumns: this.board.workflowColumns.map((column) => {
        if (column.id !== createdTask.workflowColumnId) {
          return column;
        }

        const tasks = [...column.tasks, this.mapTask(createdTask)];

        return {
          ...column,
          tasks,
          taskCount: tasks.length,
        };
      }),
    };

    this.changeDetectorRef.markForCheck();
  }

  private applyUpdatedTask(updatedTask: TaskSummaryResponse): void {
    if (!this.board) {
      return;
    }

    const targetWorkflowColumnId =
      updatedTask.workflowColumnId ||
      this.selectedWorkflowColumnId ||
      this.selectedTask?.workflowColumnId;

    if (!targetWorkflowColumnId) {
      this.loadBoardTasks(false);
      return;
    }

    const normalizedTask: TaskSummaryResponse = {
      ...updatedTask,
      workflowColumnId: targetWorkflowColumnId,
    };

    const updatedWorkflowTask = this.mapTask(normalizedTask);
    let wasTaskFound = false;

    this.board = {
      ...this.board,
      workflowColumns: this.board.workflowColumns.map((column) => {
        const existingTask = column.tasks.find((task) => task.id === normalizedTask.id);
        const tasksWithoutUpdatedTask = column.tasks.filter(
          (task) => task.id !== normalizedTask.id,
        );

        if (existingTask) {
          wasTaskFound = true;
        }

        const shouldAddUpdatedTask = column.id === normalizedTask.workflowColumnId;

        const tasks = shouldAddUpdatedTask
          ? [
              ...tasksWithoutUpdatedTask,
              {
                ...existingTask,
                ...updatedWorkflowTask,
              },
            ]
          : tasksWithoutUpdatedTask;

        return {
          ...column,
          tasks,
          taskCount: tasks.length,
        };
      }),
    };

    if (!wasTaskFound) {
      this.addCreatedTask(updatedTask);
      return;
    }

    this.changeDetectorRef.markForCheck();
  }

  onOpenTask(taskId: string): void {
    this.openTaskPanel(taskId, 'details');
  }

  closeTaskDetails(): void {
    this.isTaskDetailsVisible = false;
    this.selectedTaskId = null;
  }

  onBackToBoards(): void {
    void this.router.navigate(['/projects', this.projectId, 'boards']);
  }

  getPriorityClass(priority: string): string {
    switch (priority.trim().toLowerCase()) {
      case 'critical':
        return 'task-priority--critical';

      case 'high':
        return 'task-priority--high';

      case 'medium':
        return 'task-priority--medium';

      case 'low':
        return 'task-priority--low';

      default:
        return 'task-priority--default';
    }
  }

  getAssigneeInitial(assigneeName: string | null | undefined): string {
    const normalizedName = assigneeName?.trim();

    if (!normalizedName) {
      return '?';
    }

    return normalizedName
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (typeof error !== 'object' || error === null) {
      return fallbackMessage;
    }

    const response = error as Record<string, any>;

    switch (response['status']) {
      case 0:
        return 'Unable to connect to the BoardTask service.';

      case 401:
        return 'Your authentication token is invalid or expired.';

      case 403:
        return 'You do not have permission to perform this action.';

      case 404:
        return 'The requested board information was not found.';

      default:
        return response['error']?.message ?? response['error']?.description ?? fallbackMessage;
    }
  }
  onEditTask(event: MouseEvent, taskId: string): void {
    event.stopPropagation();
    event.preventDefault();

    this.openTaskPanel(taskId, 'edit');
  }

  private openTaskPanel(taskId: string, mode: 'edit' | 'details'): void {
    if (!this.boardId || this.loadingTaskId !== null || this.isDeletingTask) {
      return;
    }

    this.loadingTaskId = taskId;
    this.actionErrorMessage = '';
    this.changeDetectorRef.markForCheck();

    this.boardTaskService
      .getTaskById(this.boardId, taskId)
      .pipe(
        first(),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loadingTaskId = null;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (task) => {
          this.selectedTask = task;
          this.taskPanelMode = mode;
          this.selectedWorkflowColumnId = task.workflowColumnId;
          this.loadTaskAssignees();
          this.isEditTaskPanelVisible = true;
          this.changeDetectorRef.markForCheck();
        },
        error: (error) => {
          const message = this.getErrorMessage(error, 'Unable to load task details.');

          this.toastService.error('Unable to open task', message);
        },
      });
  }

  openDeleteTaskDialog(event: MouseEvent, task: WorkflowTask): void {
    event.stopPropagation();
    event.preventDefault();

    if (!task.isTaskOwner || this.isDeletingTask) {
      return;
    }

    this.taskToDelete = task;
    this.isDeleteTaskDialogVisible = true;
    this.changeDetectorRef.markForCheck();
  }

  closeDeleteTaskDialog(): void {
    if (this.isDeletingTask) {
      return;
    }

    this.isDeleteTaskDialogVisible = false;
    this.taskToDelete = null;
  }

  confirmDeleteTask(): void {
    const task = this.taskToDelete;

    if (!task || !task.isTaskOwner || !this.boardId || this.isDeletingTask) {
      return;
    }

    this.isDeletingTask = true;
    this.actionErrorMessage = '';
    this.changeDetectorRef.markForCheck();

    this.boardTaskService
      .deleteTask(this.boardId, task.id)
      .pipe(
        first(),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isDeletingTask = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.removeTaskFromBoard(task.id);

          this.isDeleteTaskDialogVisible = false;
          this.taskToDelete = null;

          if (this.selectedTask?.id === task.id) {
            this.closeEditTaskPanel();
          }

          this.toastService.success('Task deleted', `"${task.title}" was deleted successfully.`);

          this.changeDetectorRef.markForCheck();
        },
        error: (error) => {
          const message = this.getErrorMessage(error, 'Unable to delete the task.');

          this.toastService.error('Unable to delete task', message);
        },
      });
  }

  private removeTaskFromBoard(taskId: string): void {
    if (!this.board) {
      return;
    }

    this.board = {
      ...this.board,
      workflowColumns: this.board.workflowColumns.map((column) => {
        const tasks = column.tasks.filter((task) => task.id !== taskId);

        return {
          ...column,
          tasks,
          taskCount: tasks.length,
        };
      }),
    };
  }

  closeEditTaskPanel(): void {
    this.isEditTaskPanelVisible = false;
    this.selectedTask = null;
  }

  openTaskQuery(): void {
    if (!this.projectId || !this.boardId) {
      return;
    }

    void this.router.navigate(['/projects', this.projectId, 'boards', this.boardId, 'tasks']);
  }
}
