import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';

import { ButtonComponent } from '../../../../shared/component/button/button.component';
import { DropdownComponent } from '../../../../shared/component/dropdown/dropdown.component';
import { DropdownOption } from '../../../../shared/models/dropdownOption';
import { TaskQueryCondition } from '../../models/task-list';
import { TaskDetailsResponse, TaskQueryRequest } from '../../models/task-response';
import { TaskSummaryResponse } from '../../models/task-summary';
import { BoardTaskService } from '../../services/board-task.service';

interface FilterFieldOption extends DropdownOption {
  operators: DropdownOption[];
}

type TaskQueryItem = TaskDetailsResponse & {
  taskDescription?: string | null;
  taskDesc?: string | null;
};

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, ButtonComponent, DropdownComponent],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskListComponent implements OnInit {
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly boardTaskService = inject(BoardTaskService);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  projectId = '';
  boardId = '';

  tasks: TaskDetailsResponse[] = [];
  conditions: TaskQueryCondition[] = [];
  private readonly taskDescriptionsById = new Map<string, string | null>();

  taskTitleOptions: DropdownOption[] = [];
  priorityOptions: DropdownOption[] = [];
  taskTypeOptions: DropdownOption[] = [];
  assigneeOptions: DropdownOption[] = [];

  isLoading = false;
  showFilters = false;
  errorMessage = '';

  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;

  readonly logicalOperatorOptions: DropdownOption[] = [
    {
      label: 'AND',
      value: 'AND',
    },
    {
      label: 'OR',
      value: 'OR',
    },
  ];

  readonly equalsOperatorOptions: DropdownOption[] = [
    {
      label: 'Equals',
      value: 'EQUALS',
    },
    {
      label: 'Does not equal',
      value: 'NOT_EQUALS',
    },
  ];

  readonly filterFields: FilterFieldOption[] = [
    {
      label: 'Task title',
      value: 'TASK_TITLE',
      operators: this.equalsOperatorOptions,
    },
    {
      label: 'Priority',
      value: 'PRIORITY',
      operators: this.equalsOperatorOptions,
    },
    {
      label: 'Task type',
      value: 'TASK_TYPE',
      operators: this.equalsOperatorOptions,
    },
    {
      label: 'Assignee',
      value: 'ASSIGNEE',
      operators: this.equalsOperatorOptions,
    },
  ];

  ngOnInit(): void {
    this.projectId = this.activatedRoute.snapshot.paramMap.get('projectId') ?? '';

    this.boardId = this.activatedRoute.snapshot.paramMap.get('boardId') ?? '';

    if (!this.projectId || !this.boardId) {
      this.errorMessage = 'The project or board identifier is missing.';

      return;
    }

    this.loadTaskListData();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get firstItemNumber(): number {
    if (this.totalCount === 0) {
      return 0;
    }

    return (this.pageNumber - 1) * this.pageSize + 1;
  }

  get lastItemNumber(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalCount);
  }

  get activeFilterCount(): number {
    return this.conditions.filter((condition) => this.isConditionComplete(condition)).length;
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;

    if (this.showFilters && this.conditions.length === 0) {
      this.addFilter();
    }
  }

  addFilter(): void {
    this.conditions = [...this.conditions, this.createEmptyCondition()];
  }

  removeFilter(index: number): void {
    this.conditions = this.conditions.filter((_, conditionIndex) => conditionIndex !== index);
  }

  onLogicalOperatorChange(condition: TaskQueryCondition, value: string | null): void {
    condition.logicalOperator = value === 'OR' ? 'OR' : 'AND';
  }

  onFilterFieldChange(condition: TaskQueryCondition, value: string | null): void {
    condition.field = value ?? '';
    condition.operator = '';
    condition.value = '';
  }

  onFilterOperatorChange(condition: TaskQueryCondition, value: string | null): void {
    condition.operator = value ?? '';
    condition.value = '';
  }

  onFilterValueChange(condition: TaskQueryCondition, value: string | null): void {
    condition.value = value ?? '';
  }

  getOperators(field: string): DropdownOption[] {
    return this.filterFields.find((filterField) => filterField.value === field)?.operators ?? [];
  }

  getValueOptions(field: string): DropdownOption[] {
    switch (field) {
      case 'TASK_TITLE':
        return this.taskTitleOptions;

      case 'PRIORITY':
        return this.priorityOptions;

      case 'TASK_TYPE':
        return this.taskTypeOptions;

      case 'ASSIGNEE':
        return this.assigneeOptions;

      default:
        return [];
    }
  }

  applyFilters(): void {
    if (!this.areFiltersValid()) {
      this.errorMessage = 'Please select the field, operator and value for every filter.';

      return;
    }

    this.pageNumber = 1;
    this.errorMessage = '';
    this.showFilters = false;

    this.queryTasks();
  }

  clearFilters(): void {
    this.conditions = [];
    this.pageNumber = 1;
    this.errorMessage = '';

    this.queryTasks();
  }

  previousPage(): void {
    if (this.pageNumber <= 1 || this.isLoading) {
      return;
    }

    this.pageNumber--;
    this.queryTasks();
  }

  nextPage(): void {
    if (this.pageNumber >= this.totalPages || this.isLoading) {
      return;
    }

    this.pageNumber++;
    this.queryTasks();
  }

  onBackToBoard(): void {
    if (!this.projectId || !this.boardId) {
      return;
    }

    void this.router.navigate(['/projects', this.projectId, 'boards', this.boardId]);
  }

  /**
   * Loads the filter dropdown data and the first
   * page of tasks.
   */
  private loadTaskListData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      taskTitles: this.boardTaskService.getBoardTasks(this.boardId),

      priorities: this.boardTaskService.getRefTerms('PRIORITY'),

      taskTypes: this.boardTaskService.getRefTerms('TASK_TYPE'),

      projectMappings: this.boardTaskService.getProjectUserMappings(this.projectId),

      boardAccess: this.boardTaskService.getBoardAccess(this.boardId),

      taskQuery: this.boardTaskService.queryTasks(this.createQueryRequest()),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: ({ taskTitles, priorities, taskTypes, projectMappings, boardAccess, taskQuery }) => {
          this.taskTitleOptions = this.createTaskTitleOptions(taskTitles ?? []);
          this.setTaskDescriptionLookup(taskTitles ?? []);

          this.priorityOptions = priorities.map((priority) => ({
            label: priority.refTermKey,
            value: priority.refTermId,
          }));

          this.taskTypeOptions = taskTypes.map((taskType) => ({
            label: taskType.refTermKey,
            value: taskType.refTermId,
          }));

          const allowedMappingIds = new Set(
            boardAccess.map((access) => access.userProjectMappingId),
          );

          this.assigneeOptions = projectMappings
            .filter((mapping) => allowedMappingIds.has(mapping.id))
            .map((mapping) => ({
              label: mapping.userName,
              value: mapping.userId,
            }))
            .sort((first, second) => first.label.localeCompare(second.label));

          this.tasks = this.normalizeTaskDescriptions(taskQuery.items ?? []);

          this.totalCount = taskQuery.totalCount;
        },
        error: (error) => {
          this.tasks = [];
          this.totalCount = 0;
          this.errorMessage = this.getErrorMessage(error);
        },
      });
  }

  /**
   * Calls the query API when filters or pages change.
   */
  private queryTasks(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.boardTaskService
      .queryTasks(this.createQueryRequest())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          this.tasks = this.normalizeTaskDescriptions(response.items ?? []);

          this.totalCount = response.totalCount;

          this.correctCurrentPage();
        },
        error: (error) => {
          this.tasks = [];
          this.totalCount = 0;
          this.errorMessage = this.getErrorMessage(error);
        },
      });
  }

  /**
   * Creates the request sent to the backend query API.
   */
  private createQueryRequest(): TaskQueryRequest {
    return {
      conditions: this.conditions
        .filter((condition) => this.isConditionComplete(condition))
        .map((condition) => ({
          logicalOperator: condition.logicalOperator,

          field: condition.field,

          operator: condition.operator,

          value: condition.value.trim(),
        })),

      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
    };
  }

  /**
   * Ensures the selected page is valid after
   * filtering changes the total number of tasks.
   */
  private correctCurrentPage(): void {
    if (this.totalCount === 0 || this.pageNumber <= this.totalPages) {
      return;
    }

    this.pageNumber = this.totalPages;
    this.queryTasks();
  }

  private createTaskTitleOptions(tasks: TaskSummaryResponse[]): DropdownOption[] {
    const uniqueTitles = new Set(
      tasks.map((task) => task.title.trim()).filter((title) => title.length > 0),
    );

    return Array.from(uniqueTitles)
      .sort((first, second) => first.localeCompare(second))
      .map((title) => ({
        label: title,
        value: title,
      }));
  }

  private normalizeTaskDescriptions(tasks: TaskQueryItem[]): TaskDetailsResponse[] {
    return tasks.map((task) => ({
      ...task,
      description:
        task.description ??
        task.taskDescription ??
        task.taskDesc ??
        this.taskDescriptionsById.get(task.id) ??
        null,
    }));
  }

  private setTaskDescriptionLookup(tasks: TaskSummaryResponse[]): void {
    this.taskDescriptionsById.clear();

    for (const task of tasks) {
      this.taskDescriptionsById.set(task.id, task.description);
    }
  }

  private createEmptyCondition(): TaskQueryCondition {
    return {
      logicalOperator: 'AND',
      field: '',
      operator: '',
      value: '',
    };
  }

  private areFiltersValid(): boolean {
    return this.conditions.every((condition) => this.isConditionComplete(condition));
  }

  private isConditionComplete(condition: TaskQueryCondition): boolean {
    return (
      condition.field.trim().length > 0 &&
      condition.operator.trim().length > 0 &&
      condition.value.trim().length > 0
    );
  }

  private getErrorMessage(error: unknown): string {
    if (typeof error === 'object' && error !== null && 'error' in error) {
      const response = (
        error as {
          error?: {
            description?: string;
            message?: string;
            detail?: string;
          };
        }
      ).error;

      return (
        response?.description ?? response?.message ?? response?.detail ?? 'Unable to load tasks.'
      );
    }

    return 'Unable to load tasks.';
  }
}
