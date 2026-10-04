import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { BoardTaskService } from '../../services/board-task.service';

import { TaskListComponent } from './task-list.component';

describe('TaskListComponent', () => {
  let component: TaskListComponent;
  let fixture: ComponentFixture<TaskListComponent>;
  let boardTaskService: jest.Mocked<Partial<BoardTaskService>>;
  let router: jest.Mocked<Partial<Router>>;

  beforeEach(async () => {
    boardTaskService = {
      getBoardTasks: jest.fn().mockReturnValue(
        of([
          {
            id: 'task-1',
            workflowColumnId: 'todo',
            title: 'Login',
            description: 'OAuth',
            priorityName: 'High',
            taskTypeName: 'Bug',
            isTaskOwner: true,
          },
          {
            id: 'task-2',
            workflowColumnId: 'todo',
            title: 'API',
            description: null,
            priorityName: 'Low',
            taskTypeName: 'Feature',
            isTaskOwner: true,
          },
        ]),
      ),
      getRefTerms: jest.fn((type: string) =>
        of(type === 'PRIORITY' ? [{ refTermId: 'p1', refTermKey: 'High' }] : [{ refTermId: 't1', refTermKey: 'Bug' }]),
      ),
      getProjectUserMappings: jest.fn().mockReturnValue(
        of([
          { id: 'mapping-2', userId: 'user-2', userName: 'Zara' },
          { id: 'mapping-1', userId: 'user-1', userName: 'Asha' },
        ]),
      ),
      getBoardAccess: jest.fn().mockReturnValue(of([{ userProjectMappingId: 'mapping-1' }])),
      queryTasks: jest.fn().mockReturnValue(
        of({
          items: [{ id: 'task-1', title: 'Login', taskDescription: null }],
          totalCount: 1,
        }),
      ),
    };
    router = { navigate: jest.fn().mockResolvedValue(true) };

    await TestBed.configureTestingModule({
      imports: [TaskListComponent],
      providers: [
        { provide: BoardTaskService, useValue: boardTaskService },
        { provide: Router, useValue: router },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: (key: string) => (key === 'projectId' ? 'project-1' : 'board-1'),
              },
            },
          },
        },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(TaskListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load filter options and normalize task descriptions on init', () => {
    expect(component.taskTitleOptions).toEqual([
      { label: 'API', value: 'API' },
      { label: 'Login', value: 'Login' },
    ]);
    expect(component.priorityOptions).toEqual([{ label: 'High', value: 'p1' }]);
    expect(component.taskTypeOptions).toEqual([{ label: 'Bug', value: 't1' }]);
    expect(component.assigneeOptions).toEqual([{ label: 'Asha', value: 'user-1' }]);
    expect(component.tasks[0].description).toBe('OAuth');
    expect(component.totalCount).toBe(1);
  });

  it('should manage filters and report active complete filters', () => {
    component.toggleFilters();
    expect(component.showFilters).toBe(true);
    expect(component.conditions).toHaveLength(1);

    const condition = component.conditions[0];
    component.onFilterFieldChange(condition, 'PRIORITY');
    component.onFilterOperatorChange(condition, 'EQUALS');
    component.onFilterValueChange(condition, 'p1');
    component.onLogicalOperatorChange(condition, 'OR');

    expect(component.activeFilterCount).toBe(1);
    expect(component.getOperators('PRIORITY')).toEqual(component.equalsOperatorOptions);
    expect(component.getValueOptions('PRIORITY')).toEqual(component.priorityOptions);
  });

  it('should block invalid filters and query with valid filters', () => {
    component.conditions = [{ logicalOperator: 'AND', field: 'PRIORITY', operator: '', value: 'p1' }];

    component.applyFilters();
    expect(component.errorMessage).toBe('Please select the field, operator and value for every filter.');

    component.conditions = [{ logicalOperator: 'AND', field: 'PRIORITY', operator: 'EQUALS', value: ' p1 ' }];
    component.applyFilters();

    expect(component.showFilters).toBe(false);
    expect(boardTaskService.queryTasks).toHaveBeenLastCalledWith({
      conditions: [{ logicalOperator: 'AND', field: 'PRIORITY', operator: 'EQUALS', value: 'p1' }],
      pageNumber: 1,
      pageSize: 20,
    });
  });

  it('should page through results and navigate back to board', () => {
    component.totalCount = 45;
    component.pageNumber = 1;

    component.nextPage();
    component.previousPage();
    component.onBackToBoard();

    expect(component.pageNumber).toBe(1);
    expect(router.navigate).toHaveBeenCalledWith(['/projects', 'project-1', 'boards', 'board-1']);
  });

  it('should show load errors when initial data fails', () => {
    boardTaskService.queryTasks!.mockReturnValueOnce(throwError(() => ({ error: { detail: 'Query failed' } })));
    fixture = TestBed.createComponent(TaskListComponent);
    component = fixture.componentInstance;

    fixture.detectChanges();

    expect(component.tasks).toEqual([]);
    expect(component.errorMessage).toBe('Query failed');
  });
});
