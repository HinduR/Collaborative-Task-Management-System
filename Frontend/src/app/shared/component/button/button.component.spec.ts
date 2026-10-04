import { ComponentFixture, TestBed } from "@angular/core/testing";

import { ButtonComponent } from "./button.component";

describe("ButtonComponent", () => {
  let component: ButtonComponent;
  let fixture: ComponentFixture<ButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("emits click events when enabled", () => {
    const event = new MouseEvent("click");
    const emitSpy = jest.spyOn(component.buttonClick, "emit");

    component.onClick(event);

    expect(emitSpy).toHaveBeenCalledWith(event);
  });

  it("does not emit while disabled or loading", () => {
    const emitSpy = jest.spyOn(component.buttonClick, "emit");

    component.disabled = true;
    component.onClick(new MouseEvent("click"));

    component.disabled = false;
    component.loading = true;
    component.onClick(new MouseEvent("click"));

    expect(emitSpy).not.toHaveBeenCalled();
  });
});
