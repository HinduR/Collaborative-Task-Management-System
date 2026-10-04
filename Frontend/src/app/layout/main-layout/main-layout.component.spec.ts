import { ComponentFixture, TestBed } from "@angular/core/testing";
import {
  ActivatedRoute,
  Router,
} from "@angular/router";
import {
  of,
  throwError,
} from "rxjs";

import { MainLayoutComponent } from "./main-layout.component";
import { AuthService } from "../../service/auth/auth.service";
import { ToastService } from "../../shared/service/toast/toast.service";

describe("MainLayoutComponent", () => {
  let component: MainLayoutComponent;
  let fixture: ComponentFixture<MainLayoutComponent>;
  let authService: {
    getUserDetails: jest.Mock;
    logout: jest.Mock;
  };
  let router: {
    navigateByUrl: jest.Mock;
    events: ReturnType<typeof of>;
    createUrlTree: jest.Mock;
    serializeUrl: jest.Mock;
    url: string;
  };
  let toastService: {
    error: jest.Mock;
  };

  beforeEach(async () => {
    authService = {
      getUserDetails: jest.fn(() =>
        of({
          userName: "Asha",
          email: "asha@example.com",
          roles: ["Admin"],
        }),
      ),
      logout: jest.fn(() => of(null)),
    };
    router = {
      navigateByUrl: jest.fn(() => Promise.resolve(true)),
      events: of(),
      createUrlTree: jest.fn(() => ({})),
      serializeUrl: jest.fn(() => "/projects"),
      url: "/projects",
    };
    toastService = {
      error: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [MainLayoutComponent],
      providers: [
        {
          provide: AuthService,
          useValue: authService,
        },
        {
          provide: Router,
          useValue: router,
        },
        {
          provide: ActivatedRoute,
          useValue: {},
        },
        {
          provide: ToastService,
          useValue: toastService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("loads the current admin user on init", () => {
    expect(authService.getUserDetails).toHaveBeenCalledTimes(1);
    expect(component.userName).toBe("Asha");
    expect(component.isSystemAdmin).toBe(true);
    expect(component.isLoadingUser).toBe(false);
  });

  it("falls back to email and non-admin roles", () => {
    authService.getUserDetails.mockReturnValueOnce(
      of({
        userName: "",
        email: "user@example.com",
        roles: ["Member"],
      }),
    );

    component.ngOnInit();

    expect(component.userName).toBe("user@example.com");
    expect(component.isSystemAdmin).toBe(false);
  });

  it("shows a fallback user name when current user loading fails", () => {
    authService.getUserDetails.mockReturnValueOnce(
      throwError(() => new Error("failed")),
    );

    component.ngOnInit();

    expect(component.userName).toBe("User");
    expect(component.isLoadingUser).toBe(false);
    expect(toastService.error).toHaveBeenCalledWith(
      "User details unavailable",
      "Unable to load your user details.",
    );
  });

  it("toggles the sidebar collapsed state", () => {
    component.onSidebarToggle();
    expect(component.isSidebarCollapsed).toBe(true);

    component.onSidebarToggle();
    expect(component.isSidebarCollapsed).toBe(false);
  });

  it("logs out and navigates to login", () => {
    component.onLogout();

    expect(authService.logout).toHaveBeenCalledTimes(1);
    expect(router.navigateByUrl).toHaveBeenCalledWith("/login");
    expect(component.isLoggingOut).toBe(false);
  });

  it("does not start a second logout while one is in progress", () => {
    component.isLoggingOut = true;

    component.onLogout();

    expect(authService.logout).not.toHaveBeenCalled();
  });

  it("shows a toast when logout fails", () => {
    authService.logout.mockReturnValueOnce(
      throwError(() => new Error("failed")),
    );

    component.onLogout();

    expect(component.isLoggingOut).toBe(false);
    expect(toastService.error).toHaveBeenCalledWith(
      "Logout failed",
      "Unable to log out. Please try again.",
    );
  });
});
