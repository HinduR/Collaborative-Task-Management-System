import { ComponentFixture, TestBed } from "@angular/core/testing";
import { ActivatedRoute, Router } from "@angular/router";
import { signal } from "@angular/core";
import { of } from "rxjs";

import { HeaderComponent } from "./header.component";
import { ThemeService } from "../../shared/service/theme/theme.service";

describe("HeaderComponent", () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;
  let darkModeSignal = signal(false);
  let themeService: {
    isDarkMode: typeof darkModeSignal;
    toggleTheme: jest.Mock;
  };

  beforeEach(async () => {
    darkModeSignal = signal(false);
    themeService = {
      isDarkMode: darkModeSignal,
      toggleTheme: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [HeaderComponent],
      providers: [
        {
          provide: ThemeService,
          useValue: themeService,
        },
        {
          provide: Router,
          useValue: {
            events: of(),
            url: "/projects",
            createUrlTree: jest.fn(() => ({})),
            serializeUrl: jest.fn(() => "/projects"),
          },
        },
        {
          provide: ActivatedRoute,
          useValue: {},
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("exposes theme icon and label for light and dark modes", () => {
    expect(component.themeToggleIcon).toBe("pi pi-moon");
    expect(component.themeToggleLabel).toBe("Switch to dark mode");

    darkModeSignal.set(true);

    expect(component.themeToggleIcon).toBe("pi pi-sun");
    expect(component.themeToggleLabel).toBe("Switch to light mode");
  });

  it("emits header actions and toggles the theme", () => {
    const sidebarSpy = jest.spyOn(component.sidebarToggle, "emit");
    const logoutSpy = jest.spyOn(component.logout, "emit");

    component.onSidebarToggle();
    component.onThemeToggle();
    component.onLogout();

    expect(sidebarSpy).toHaveBeenCalled();
    expect(themeService.toggleTheme).toHaveBeenCalled();
    expect(logoutSpy).toHaveBeenCalled();
  });
});
