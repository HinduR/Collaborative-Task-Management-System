import { ComponentFixture, TestBed } from "@angular/core/testing";

import { CardComponent } from "./card.component";

describe("CardComponent", () => {
  let component: CardComponent;
  let fixture: ComponentFixture<CardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("builds style classes from the task type and custom class", () => {
    component.styleClass = "custom-card";

    component.type = " Bug ";
    expect(component.cardStyleClass).toBe("shared-card card-type-bug custom-card");

    component.type = "feature";
    expect(component.cardStyleClass).toContain("card-type-feature");

    component.type = "story";
    expect(component.cardStyleClass).toContain("card-type-story");

    component.type = "task";
    expect(component.cardStyleClass).toContain("card-type-task");

    component.type = "unknown";
    component.styleClass = "";
    expect(component.cardStyleClass).toBe("shared-card card-type-default");
  });
});
