import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from "@angular/core";
import { FormControl, FormsModule, ReactiveFormsModule } from "@angular/forms";
import {
  CdkDragDrop,
  DragDropModule,
  moveItemInArray,
} from "@angular/cdk/drag-drop";

import { ButtonComponent } from "../../../../shared/component/button/button.component";
import { MultiSelectComponent } from "../../../../shared/component/multi-select/multi-select.component";
import {
  WorkflowColumn,
  WorkflowColumnChange,
  WorkflowColumnSaveRequest,
} from "../../models/workflow-column-request";
import { DropdownOption } from "../../../../shared/models/dropdownOption";

@Component({
  selector: "app-workflow-column",
  standalone: true,
  imports: [
    FormsModule,
    DragDropModule,
    ButtonComponent,
    MultiSelectComponent,
    ReactiveFormsModule,
  ],
  templateUrl: "./workflow-column.component.html",
  styleUrl: "./workflow-column.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkflowColumnComponent implements OnChanges {
  @Input({ required: true })
  columns: WorkflowColumn[] = [];
  @Input()
  roleOptions: DropdownOption[] = [];

  @Input()
  maximumColumnCount = 5;

  @Input()
  disabled = false;

  @Output()
  readonly saveChanges = new EventEmitter<WorkflowColumnSaveRequest[]>();

  @Output()
  readonly cancelled = new EventEmitter<void>();

  editableColumns: WorkflowColumnChange[] = [];
  validationMessage = "";

  ngOnChanges(changes: SimpleChanges): void {
    if (changes["columns"]) {
      this.resetColumns();
    }
  }

  get canAddColumn(): boolean {
    return (
      !this.disabled && this.editableColumns.length < this.maximumColumnCount
    );
  }

  get canSave(): boolean {
    return (
      !this.disabled &&
      this.editableColumns.length > 0 &&
      !this.hasEmptyColumnName() &&
      !this.hasDuplicateNames()
    );
  }

  addColumn(): void {
    if (!this.canAddColumn) {
      return;
    }

    this.editableColumns = [
      ...this.editableColumns,
      {
        id: this.generateTemporaryId(),
        name: "",
        sortOrder: this.editableColumns.length + 1,
        taskCount: 0,
        isNew: true,
        roleControl: new FormControl<string[]>([], {
          nonNullable: true,
        }),
      },
    ];

    this.validateColumns();
  }
  deleteColumn(columnId: string): void {
    if (this.disabled) {
      return;
    }

    const selectedColumn = this.editableColumns.find(
      (column) => column.id === columnId,
    );

    if (!selectedColumn) {
      return;
    }

    if (selectedColumn.taskCount > 0) {
      this.validationMessage =
        "Move or delete all tasks before deleting this column.";
      return;
    }

    /*
     * This only removes the column from the editable UI.
     * The parent must send the deleted IDs to the backend
     * or use a separate delete-column API.
     */
    this.editableColumns = this.editableColumns.filter(
      (column) => column.id !== columnId,
    );

    this.updateSortOrders();
    this.validateColumns();
  }

  drop(event: CdkDragDrop<WorkflowColumnChange[]>): void {
    if (this.disabled || event.previousIndex === event.currentIndex) {
      return;
    }

    const reorderedColumns = [...this.editableColumns];

    moveItemInArray(reorderedColumns, event.previousIndex, event.currentIndex);

    this.editableColumns = reorderedColumns;
    this.updateSortOrders();
    this.validateColumns();
  }

  onColumnNameChange(): void {
    this.validateColumns();
  }

  save(): void {
    this.validateColumns();

    if (!this.canSave) {
      return;
    }

    const request: WorkflowColumnSaveRequest[] = this.editableColumns.map(
      (column) => ({
        id: column.isNew ? null : column.id,
        columnName: column.name.trim(),
        roleIds: [...new Set(column.roleControl.getRawValue())],
      }),
    );

    this.saveChanges.emit(request);
  }
  cancel(): void {
    if (this.disabled) {
      return;
    }

    this.resetColumns();
    this.cancelled.emit();
  }

  private resetColumns(): void {
    this.editableColumns = [...this.columns]
      .sort(
        (firstColumn, secondColumn) =>
          firstColumn.sortOrder - secondColumn.sortOrder,
      )
      .map((column) => ({
        id: column.id,
        name: column.name,
        sortOrder: column.sortOrder,
        taskCount: column.taskCount ?? column.tasks?.length ?? 0,
        isNew: false,
        roleControl: new FormControl<string[]>(column.roleIds ?? [], {
          nonNullable: true,
        }),
      }));

    this.validationMessage = "";
  }

  private updateSortOrders(): void {
    this.editableColumns = this.editableColumns.map((column, index) => ({
      ...column,
      sortOrder: index + 1,
    }));
  }

  private validateColumns(): void {
    if (this.editableColumns.length === 0) {
      this.validationMessage = "At least one workflow column is required.";
      return;
    }

    if (this.hasEmptyColumnName()) {
      this.validationMessage = "Workflow column name is required.";
      return;
    }

    if (this.hasDuplicateNames()) {
      this.validationMessage =
        "Workflow column names must be unique within the board.";
      return;
    }

    this.validationMessage = "";
  }

  private hasEmptyColumnName(): boolean {
    return this.editableColumns.some((column) => !column.name.trim());
  }

  private hasDuplicateNames(): boolean {
    const normalizedNames = this.editableColumns
      .map((column) => column.name.trim().toLowerCase())
      .filter(Boolean);

    return new Set(normalizedNames).size !== normalizedNames.length;
  }

  private generateTemporaryId(): string {
    if (
      typeof crypto !== "undefined" &&
      typeof crypto.randomUUID === "function"
    ) {
      return `temporary-column-${crypto.randomUUID()}`;
    }

    return (
      `temporary-column-${Date.now()}-` + Math.random().toString(36).slice(2)
    );
  }
}
