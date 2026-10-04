import { ComponentFixture, TestBed } from "@angular/core/testing";

import { DialogComponent } from "./dialog.component";

describe("DialogComponent", () => {
  let component: DialogComponent;
  let fixture: ComponentFixture<DialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DialogComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("emits visibility changes", () => {
    const visibleChangeSpy = jest.spyOn(component.visibleChange, "emit");
    const closedSpy = jest.spyOn(component.closed, "emit");

    component.onVisibleChange(true);

    expect(component.visible).toBe(true);
    expect(visibleChangeSpy).toHaveBeenCalledWith(true);
    expect(closedSpy).not.toHaveBeenCalled();
  });

  it("emits closed when hidden", () => {
    const visibleChangeSpy = jest.spyOn(component.visibleChange, "emit");
    const closedSpy = jest.spyOn(component.closed, "emit");

    component.onVisibleChange(false);

    expect(component.visible).toBe(false);
    expect(visibleChangeSpy).toHaveBeenCalledWith(false);
    expect(closedSpy).toHaveBeenCalled();
  });
});
