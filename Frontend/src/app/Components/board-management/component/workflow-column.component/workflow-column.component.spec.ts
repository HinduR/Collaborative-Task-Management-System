import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FormControl } from "@angular/forms";

import { WorkflowColumnComponent } from "./workflow-column.component";
import { WorkflowColumn } from "../../models/workflow-column-request";

describe("WorkflowColumnComponent", () => {
  let component: WorkflowColumnComponent;
  let fixture: ComponentFixture<WorkflowColumnComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkflowColumnComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(WorkflowColumnComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should sort columns and build editable role controls", () => {
    component.columns = [
      { id: "done", name: "Done", sortOrder: 2, taskCount: 1, roleIds: ["qa"], tasks: [] },
      { id: "todo", name: "Todo", sortOrder: 1, taskCount: 0, roleIds: ["dev"], tasks: [] },
    ] as WorkflowColumn[];

    component.ngOnChanges({ columns: { currentValue: component.columns } } as any);

    expect(component.editableColumns.map((column) => column.id)).toEqual(["todo", "done"]);
    expect(component.editableColumns[0].roleControl.value).toEqual(["dev"]);
  });

  it("should add a new column when below the maximum", () => {
    component.maximumColumnCount = 2;
    component.columns = [{ id: "todo", name: "Todo", sortOrder: 1, taskCount: 0, tasks: [] }] as WorkflowColumn[];
    component.ngOnChanges({ columns: { currentValue: component.columns } } as any);

    component.addColumn();

    expect(component.editableColumns).toHaveLength(2);
    expect(component.editableColumns[1].isNew).toBe(true);
    expect(component.validationMessage).toBe("Workflow column name is required.");
  });

  it("should prevent deleting columns that contain tasks", () => {
    component.editableColumns = [
      { id: "doing", name: "Doing", sortOrder: 1, taskCount: 2, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
    ] as any;

    component.deleteColumn("doing");

    expect(component.validationMessage).toBe("Move or delete all tasks before deleting this column.");
    expect(component.editableColumns).toHaveLength(1);
  });

  it("should validate duplicate names and avoid emitting invalid saves", () => {
    jest.spyOn(component.saveChanges, "emit");
    component.columns = [
      { id: "one", name: "Todo", sortOrder: 1, taskCount: 0, tasks: [] },
      { id: "two", name: " todo ", sortOrder: 2, taskCount: 0, tasks: [] },
    ] as WorkflowColumn[];
    component.ngOnChanges({ columns: { currentValue: component.columns } } as any);

    component.save();

    expect(component.validationMessage).toBe("Workflow column names must be unique within the board.");
    expect(component.saveChanges.emit).not.toHaveBeenCalled();
  });

  it("should emit trimmed save requests with unique role ids", () => {
    jest.spyOn(component.saveChanges, "emit");
    component.columns = [{ id: "todo", name: " Todo ", sortOrder: 1, taskCount: 0, roleIds: ["dev", "dev"], tasks: [] }] as WorkflowColumn[];
    component.ngOnChanges({ columns: { currentValue: component.columns } } as any);

    component.save();

    expect(component.saveChanges.emit).toHaveBeenCalledWith([
      { id: "todo", columnName: "Todo", roleIds: ["dev"] },
    ]);
  });

  it("deletes empty columns and updates sort order", () => {
    component.editableColumns = [
      { id: "one", name: "One", sortOrder: 1, taskCount: 0, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
      { id: "two", name: "Two", sortOrder: 2, taskCount: 0, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
    ] as any;

    component.deleteColumn("one");

    expect(component.editableColumns).toHaveLength(1);
    expect(component.editableColumns[0].sortOrder).toBe(1);
    expect(component.validationMessage).toBe("");
  });

  it("validates that at least one column remains", () => {
    component.editableColumns = [
      { id: "one", name: "One", sortOrder: 1, taskCount: 0, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
    ] as any;

    component.deleteColumn("one");

    expect(component.validationMessage).toBe("At least one workflow column is required.");
    expect(component.canSave).toBe(false);
  });

  it("reorders columns on drop", () => {
    component.editableColumns = [
      { id: "one", name: "One", sortOrder: 1, taskCount: 0, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
      { id: "two", name: "Two", sortOrder: 2, taskCount: 0, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
    ] as any;

    component.drop({ previousIndex: 0, currentIndex: 1 } as any);

    expect(component.editableColumns.map((column) => column.id)).toEqual(["two", "one"]);
    expect(component.editableColumns.map((column) => column.sortOrder)).toEqual([1, 2]);
  });

  it("respects disabled guards", () => {
    const cancelledSpy = jest.spyOn(component.cancelled, "emit");
    const saveSpy = jest.spyOn(component.saveChanges, "emit");
    component.disabled = true;
    component.editableColumns = [
      { id: "one", name: "One", sortOrder: 1, taskCount: 0, isNew: false, roleControl: new FormControl<string[]>([], { nonNullable: true }) },
    ] as any;

    component.addColumn();
    component.deleteColumn("one");
    component.drop({ previousIndex: 0, currentIndex: 0 } as any);
    component.save();
    component.cancel();

    expect(component.editableColumns).toHaveLength(1);
    expect(saveSpy).not.toHaveBeenCalled();
    expect(cancelledSpy).not.toHaveBeenCalled();
    expect(component.canAddColumn).toBe(false);
    expect(component.canSave).toBe(false);
  });

  it("resets and emits cancel when enabled", () => {
    const cancelledSpy = jest.spyOn(component.cancelled, "emit");
    component.columns = [
      { id: "one", name: "One", sortOrder: 1, taskCount: 0, tasks: [] },
    ] as WorkflowColumn[];
    component.ngOnChanges({ columns: { currentValue: component.columns } } as any);
    component.editableColumns[0].name = "";

    component.cancel();

    expect(component.editableColumns[0].name).toBe("One");
    expect(cancelledSpy).toHaveBeenCalled();
  });
});
