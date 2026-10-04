import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';

import { ButtonComponent } from '../../../../shared/component/button/button.component';
import { DropdownComponent } from '../../../../shared/component/dropdown/dropdown.component';
import { DropdownOption } from '../../../../shared/models/dropdownOption';
import { ToastService } from '../../../../shared/service/toast/toast.service';

import { TaskSummaryResponse } from '../../models/task-summary';
import { CreateTaskRequest } from '../../models/create-task-request';
import { TaskForm } from '../../models/task-form.model';
import { TaskCommentResponse, TaskDetailsResponse } from '../../models/task-response';
import { BoardTaskService } from '../../services/board-task.service';
import { DialogComponent } from '../../../../shared/component/dialog/dialog.component';

@Component({
  selector: 'app-task-panel',
  standalone: true,
  imports: [FormsModule, ButtonComponent, DialogComponent, DropdownComponent],
  templateUrl: './task-panel.component.html',
  styleUrl: './task-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskPanelComponent implements OnInit, OnChanges {
  private readonly boardTaskService = inject(BoardTaskService);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastService = inject(ToastService);

  @Input({ required: true }) boardId = '';

  @Input() mode: 'create' | 'edit' | 'details' = 'create';

  @Input() task: TaskDetailsResponse | null = null;

  @Input() selectedWorkflowColumnId: string | null = null;

  @Input() workflowColumnOptions: DropdownOption[] = [];

  @Input() assigneeOptions: DropdownOption[] = [];

  @Output() readonly taskCreated = new EventEmitter<TaskSummaryResponse>();
  @Output()
  readonly taskUpdated = new EventEmitter<TaskSummaryResponse>();

  @Output() readonly cancelled = new EventEmitter<void>();

  priorityOptions: DropdownOption[] = [];
  taskTypeOptions: DropdownOption[] = [];
  showDeleteCommentDialog = false;
  commentToDelete: TaskCommentResponse | null = null;

  isLoadingOptions = false;
  isSaving = false;
  errorMessage = '';
  comments: TaskCommentResponse[] = [];
  newComment = '';
  editingCommentId: string | null = null;
  editingCommentText = '';
  isSavingComment = false;

  taskForm: TaskForm = this.createEmptyForm();

  get isEditMode(): boolean {
    return this.mode === 'edit';
  }

  get isDetailsMode(): boolean {
    return this.mode === 'details';
  }

  get isTaskMode(): boolean {
    return this.isEditMode || this.isDetailsMode;
  }

  get isFormDisabled(): boolean {
    return this.isSaving || this.isDetailsMode;
  }

  get panelTitle(): string {
    if (this.isDetailsMode) {
      return 'Task Details';
    }

    return this.isEditMode ? 'Edit Task' : 'Create Task';
  }

  get saveButtonLabel(): string {
    if (this.isSaving) {
      return this.isEditMode ? 'Saving...' : 'Creating...';
    }

    return this.isEditMode ? 'Save Changes' : 'Create Task';
  }

  ngOnInit(): void {
    this.loadDropdownOptions();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['task'] && this.isTaskMode && this.task) {
      this.populateTaskForm();
      return;
    }

    if (changes['selectedWorkflowColumnId'] && this.selectedWorkflowColumnId) {
      this.taskForm.workflowColumnId = this.selectedWorkflowColumnId;
    }
  }

  saveTask(): void {
    if (this.isDetailsMode) {
      return;
    }

    this.errorMessage = '';

    const title = this.taskForm.title.trim();

    if (!title) {
      this.errorMessage = 'Task title is required.';
      return;
    }

    if (title.length > 200) {
      this.errorMessage = 'Task title cannot exceed 200 characters.';
      return;
    }

    if (!this.taskForm.priorityRefTermKey) {
      this.errorMessage = 'Priority is required.';
      return;
    }

    if (!this.taskForm.taskTypeRefTermKey) {
      this.errorMessage = 'Task type is required.';
      return;
    }

    if (!this.taskForm.workflowColumnId) {
      this.errorMessage = 'Workflow column is required.';
      return;
    }

    const request: CreateTaskRequest = {
      title,
      description: this.taskForm.description.trim() || null,
      priorityRefTermKey: this.taskForm.priorityRefTermKey,
      taskTypeRefTermKey: this.taskForm.taskTypeRefTermKey,
      assigneeUserId: this.taskForm.assigneeUserId,
      workflowColumnId: this.taskForm.workflowColumnId,
    };

    if (this.isEditMode) {
      this.updateTask(request);
      return;
    }

    this.createTask(request);
  }

  cancel(): void {
    if (this.isSaving) {
      return;
    }

    this.resetForm();
    this.cancelled.emit();
  }

  private createTask(request: CreateTaskRequest): void {
    this.isSaving = true;

    this.boardTaskService
      .createTask(this.boardId, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSaving = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (createdTask) => {
          this.toastService.success(
            'Task created',
            `"${createdTask.title}" was added to the board.`,
          );

          this.taskCreated.emit(createdTask);
          this.resetForm();
        },
        error: (error) => {
          this.errorMessage = this.getErrorMessage(error);

          this.toastService.error('Unable to create task', this.errorMessage);
        },
      });
  }

  private updateTask(request: CreateTaskRequest): void {
    if (!this.task) {
      this.errorMessage = 'The selected task could not be found.';
      return;
    }

    this.isSaving = true;

    this.boardTaskService
      .updateTask(this.boardId, this.task.id, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSaving = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (updatedTask) => {
          this.toastService.success(
            'Task updated',
            `"${updatedTask.title}" was updated successfully.`,
          );

          this.taskUpdated.emit({
            ...updatedTask,
            workflowColumnId: updatedTask.workflowColumnId || request.workflowColumnId,
          });
          this.resetForm();
        },
        error: (error) => {
          this.errorMessage = this.getErrorMessage(error);

          this.toastService.error('Unable to update task', this.errorMessage);
        },
      });
  }

  private loadDropdownOptions(): void {
    this.isLoadingOptions = true;
    this.errorMessage = '';

    forkJoin({
      priorities: this.boardTaskService.getRefTerms('PRIORITY'),
      taskTypes: this.boardTaskService.getRefTerms('TASK_TYPE'),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoadingOptions = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: ({ priorities, taskTypes }) => {
          this.priorityOptions = priorities.map((term) => ({
            label: term.description || term.refTermKey,
            value: term.refTermKey,
          }));

          this.taskTypeOptions = taskTypes.map((term) => ({
            label: term.description || term.refTermKey,
            value: term.refTermKey,
          }));

          // Run again after options are available so edit
          // dropdowns receive their correct values.
          if (this.isTaskMode && this.task) {
            this.populateTaskForm();
          }
        },
        error: (error) => {
          this.priorityOptions = [];
          this.taskTypeOptions = [];
          this.errorMessage = this.getErrorMessage(error);

          this.toastService.error('Unable to load task options', this.errorMessage);
        },
      });
  }

  private populateTaskForm(): void {
    if (!this.task) {
      return;
    }

    this.comments = [...(this.task.comments ?? [])];
    this.taskForm = {
      title: this.task.title,
      description: this.task.description ?? '',

      // Temporary name-to-key mapping.
      // Prefer returning these keys from the details API.
      priorityRefTermKey: this.findOptionValue(this.priorityOptions, this.task.priorityName),

      taskTypeRefTermKey: this.findOptionValue(this.taskTypeOptions, this.task.taskTypeName),

      assigneeUserId: this.task.assigneeUserId,

      // Pass the task's column ID through
      // selectedWorkflowColumnId from the parent.
      workflowColumnId: this.selectedWorkflowColumnId,
    };

    this.errorMessage = '';
    this.changeDetectorRef.markForCheck();
  }

  private findOptionValue(options: DropdownOption[], label: string): string | null {
    const matchingOption = options.find(
      (option) => option.label.trim().toLowerCase() === label.trim().toLowerCase(),
    );

    return matchingOption ? String(matchingOption.value) : null;
  }

  private resetForm(): void {
    this.taskForm = this.createEmptyForm();
    this.errorMessage = '';
    this.changeDetectorRef.markForCheck();
  }

  private createEmptyForm(): TaskForm {
    return {
      title: '',
      description: '',
      priorityRefTermKey: null,
      taskTypeRefTermKey: null,
      assigneeUserId: null,
      workflowColumnId: this.selectedWorkflowColumnId,
    };
  }

  private getErrorMessage(error: unknown): string {
    if (typeof error === 'object' && error !== null) {
      const response = error as Record<string, any>;

      if (response['status'] === 0) {
        return 'Unable to connect to the service.';
      }

      if (response['status'] === 400) {
        return (
          response['error']?.message ??
          response['error']?.description ??
          'Please check the task details.'
        );
      }

      if (response['status'] === 401) {
        return 'Your session has expired.';
      }

      if (response['status'] === 403) {
        return this.isEditMode
          ? 'You do not have permission to update this task.'
          : 'You do not have permission to create this task.';
      }
    }

    return this.isEditMode ? 'Unable to update the task.' : 'Unable to create the task.';
  }

  addComment(): void {
    if (!this.task) {
      return;
    }

    const commentText = this.newComment.trim();

    if (!commentText) {
      return;
    }

    this.isSavingComment = true;

    this.boardTaskService
      .createTaskComment(this.boardId, this.task.id, { comment: commentText })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingComment = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (createdComment) => {
          this.comments = [...this.comments, createdComment];
          this.newComment = '';

          this.toastService.success('Comment added', 'The comment was added successfully.');
        },
        error: (error) => {
          this.toastService.error('Unable to add comment', this.getErrorMessage(error));
        },
      });
  }

  startEditingComment(comment: TaskCommentResponse): void {
    if (!comment.isCommentOwner) {
      return;
    }

    this.editingCommentId = comment.id;
    this.editingCommentText = comment.comment;
  }

  cancelEditingComment(): void {
    this.editingCommentId = null;
    this.editingCommentText = '';
  }

  updateComment(comment: TaskCommentResponse): void {
    const commentText = this.editingCommentText.trim();

    if (!commentText || !comment.isCommentOwner) {
      return;
    }

    this.isSavingComment = true;

    this.boardTaskService
      .updateTaskComment(this.boardId, this.task!.id, comment.id, { comment: commentText })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingComment = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (updatedComment) => {
          this.comments = this.comments.map((item) =>
            item.id === updatedComment.id ? updatedComment : item,
          );

          this.cancelEditingComment();

          this.toastService.success('Comment updated', 'The comment was updated successfully.');
        },
        error: (error) => {
          this.toastService.error('Unable to update comment', this.getErrorMessage(error));
        },
      });
  }

  openDeleteCommentDialog(comment: TaskCommentResponse): void {
    if (!comment.isCommentOwner || this.isSavingComment) {
      return;
    }

    this.commentToDelete = comment;
    this.showDeleteCommentDialog = true;
  }

  cancelDeleteComment(): void {
    if (this.isSavingComment) {
      return;
    }

    this.showDeleteCommentDialog = false;
    this.commentToDelete = null;
  }

  confirmDeleteComment(): void {
    const comment = this.commentToDelete;

    if (!this.task || !comment || !comment.isCommentOwner || this.isSavingComment) {
      return;
    }

    this.isSavingComment = true;

    this.boardTaskService
      .deleteTaskComment(this.boardId, this.task.id, comment.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingComment = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.comments = this.comments.filter((item) => item.id !== comment.id);

          if (this.editingCommentId === comment.id) {
            this.cancelEditingComment();
          }

          this.showDeleteCommentDialog = false;
          this.commentToDelete = null;

          this.toastService.success('Comment deleted', 'The comment was deleted successfully.');
        },
        error: (error) => {
          this.toastService.error('Unable to delete comment', this.getErrorMessage(error));
        },
      });
  }
}
