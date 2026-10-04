import { ComponentFixture, TestBed } from "@angular/core/testing";
import { Router } from "@angular/router";
import {
  of,
  throwError,
} from "rxjs";

import { ProjectComponent } from "./project.component";
import { ToastService } from "../../../shared/service/toast/toast.service";
import { UserMappingService } from "../../user-mapping/service/user-mapping-service";

describe("ProjectComponent", () => {
  let component: ProjectComponent;
  let fixture: ComponentFixture<ProjectComponent>;
  let userMappingService: {
    getProjectList: jest.Mock;
  };
  let toastService: {
    error: jest.Mock;
    warning: jest.Mock;
  };
  let router: {
    navigate: jest.Mock;
  };

  const assignedProject = {
    id: 7,
    name: "Core",
    description: "Core project",
    isAssigned: true,
  };

  beforeEach(async () => {
    userMappingService = {
      getProjectList: jest.fn(() => of([assignedProject])),
    };
    toastService = {
      error: jest.fn(),
      warning: jest.fn(),
    };
    router = {
      navigate: jest.fn(() => Promise.resolve(true)),
    };

    await TestBed.configureTestingModule({
      imports: [ProjectComponent],
      providers: [
        {
          provide: UserMappingService,
          useValue: userMappingService,
        },
        {
          provide: ToastService,
          useValue: toastService,
        },
        {
          provide: Router,
          useValue: router,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("loads assigned projects on init", () => {
    expect(userMappingService.getProjectList).toHaveBeenCalledTimes(1);
    expect(component.projects).toEqual([assignedProject]);
    expect(component.isLoading).toBe(false);
  });

  it("uses an empty project list when the service returns null", () => {
    userMappingService.getProjectList.mockReturnValue(of(null));

    component.loadProjects();

    expect(component.projects).toEqual([]);
    expect(component.isLoading).toBe(false);
  });

  it("shows an error toast when projects fail to load", () => {
    userMappingService.getProjectList.mockReturnValue(
      throwError(() => new Error("network")),
    );

    component.loadProjects();

    expect(component.projects).toEqual([]);
    expect(component.isLoading).toBe(false);
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load projects",
      "Please try again later.",
    );
  });

  it("opens assigned projects", () => {
    component.openProject(assignedProject);

    expect(router.navigate).toHaveBeenCalledWith([
      "/projects",
      assignedProject.id,
      "boards",
    ]);
    expect(toastService.warning).not.toHaveBeenCalled();
  });

  it("blocks unassigned projects", () => {
    component.openProject({
      ...assignedProject,
      isAssigned: false,
    });

    expect(router.navigate).not.toHaveBeenCalled();
    expect(toastService.warning).toHaveBeenCalledWith(
      "Access denied",
      "You are not assigned to this project.",
    );
  });

  it("opens projects from Enter and Space key presses only", () => {
    const enterEvent = {
      key: "Enter",
      preventDefault: jest.fn(),
    } as unknown as KeyboardEvent;
    const spaceEvent = {
      key: " ",
      preventDefault: jest.fn(),
    } as unknown as KeyboardEvent;
    const escapeEvent = {
      key: "Escape",
      preventDefault: jest.fn(),
    } as unknown as KeyboardEvent;

    component.handleKeyDown(escapeEvent, assignedProject);
    component.handleKeyDown(enterEvent, assignedProject);
    component.handleKeyDown(spaceEvent, assignedProject);

    expect(escapeEvent.preventDefault).not.toHaveBeenCalled();
    expect(enterEvent.preventDefault).toHaveBeenCalled();
    expect(spaceEvent.preventDefault).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledTimes(2);
  });
});
