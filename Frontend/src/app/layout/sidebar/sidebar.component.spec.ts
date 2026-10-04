import { ComponentFixture, TestBed } from "@angular/core/testing";
import {
  NavigationEnd,
  Router,
} from "@angular/router";
import { Subject } from "rxjs";

import { SidebarComponent } from "./sidebar.component";

describe("SidebarComponent", () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;
  let routerEvents: Subject<NavigationEnd>;
  let router: {
    events: Subject<NavigationEnd>;
    url: string;
    navigateByUrl: jest.Mock;
  };

  beforeEach(async () => {
    routerEvents = new Subject<NavigationEnd>();
    router = {
      events: routerEvents,
      url: "/projects",
      navigateByUrl: jest.fn(() => Promise.resolve(true)),
    };

    await TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        {
          provide: Router,
          useValue: router,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("filters admin-only items", () => {
    const adminItem = component.navigationItems.find((item) => item.requiredRole)!;
    const projectItem = component.navigationItems.find((item) => !item.requiredRole)!;

    expect(component.canDisplay(projectItem)).toBe(true);
    expect(component.canDisplay(adminItem)).toBe(false);

    component.isSystemAdmin = true;

    expect(component.canDisplay(adminItem)).toBe(true);
  });

  it("navigates and detects active routes", () => {
    const projectItem = component.navigationItems[0];

    expect(component.isActive(projectItem)).toBe(true);

    routerEvents.next(new NavigationEnd(1, "/projects/7", "/projects/7"));

    expect(component.isActive(projectItem)).toBe(true);

    component.navigateTo(projectItem);

    expect(router.navigateByUrl).toHaveBeenCalledWith("/projects");
  });
});
