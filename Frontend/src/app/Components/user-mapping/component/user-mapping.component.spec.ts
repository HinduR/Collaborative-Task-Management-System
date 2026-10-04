import { ComponentFixture, TestBed } from "@angular/core/testing";
import { of, throwError } from "rxjs";
import { ToastService } from "src/app/shared/service/toast/toast.service";
import { UserMappingService } from "../service/user-mapping-service";

import { UserMappingComponent } from "./user-mapping.component";

describe("UserMappingComponent", () => {
  let component: UserMappingComponent;
  let fixture: ComponentFixture<UserMappingComponent>;
  let userMappingService: jest.Mocked<Partial<UserMappingService>>;
  let toastService: jest.Mocked<Partial<ToastService>>;

  beforeEach(async () => {
    userMappingService = {
      getUsers: jest.fn().mockReturnValue(of([{ label: "Asha", value: "user-1" }])),
      getProjects: jest.fn().mockReturnValue(of([{ label: "Round Table", value: "project-1" }])),
      getRoles: jest.fn().mockReturnValue(of([{ label: "Admin", value: "role-1" }])),
      getUserProjectMappings: jest.fn().mockReturnValue(
        of([{ userId: "user-1", projectIdList: ["project-1"] }]),
      ),
      saveUserProjectMapping: jest.fn(),
      assignUserRole: jest.fn(),
    };
    toastService = {
      success: jest.fn(),
      error: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [UserMappingComponent],
      providers: [
        { provide: UserMappingService, useValue: userMappingService },
        { provide: ToastService, useValue: toastService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserMappingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should load users, projects, roles, and mappings", () => {
    expect(component.users).toEqual([{ label: "Asha", value: "user-1" }]);
    expect(component.projects).toEqual([{ label: "Round Table", value: "project-1" }]);
    expect(component.roles).toEqual([{ label: "Admin", value: "role-1" }]);
    expect(component.userProjectMappings).toEqual([
      { userId: "user-1", projectIdList: ["project-1"] },
    ]);
  });

  it("should show an error when initial data fails", async () => {
    TestBed.resetTestingModule();
    userMappingService.getUsers!.mockReturnValueOnce(throwError(() => new Error("fail")));
    await TestBed.configureTestingModule({
      imports: [UserMappingComponent],
      providers: [
        { provide: UserMappingService, useValue: userMappingService },
        { provide: ToastService, useValue: toastService },
      ],
    }).compileComponents();
    const failedFixture = TestBed.createComponent(UserMappingComponent);
    const failedComponent = failedFixture.componentInstance;

    failedFixture.detectChanges();

    expect(failedComponent.errorMessage).toBe("Unable to load user mapping details.");
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to load mappings",
      "Unable to load user mapping details.",
    );
  });

  it("should open project dialog and prefill projects for a selected user", () => {
    component.openProjectMappingDialog();
    component.onProjectUserChange("USER-1");

    expect(component.showProjectMappingDialog).toBe(true);
    expect(component.mappingForm.controls.projectIds.value).toEqual(["project-1"]);
  });

  it("should save project mappings and update local state", () => {
    userMappingService.saveUserProjectMapping!.mockReturnValue(of(undefined));
    component.openProjectMappingDialog();
    component.mappingForm.setValue({
      userId: "user-2",
      projectIds: ["project-2"],
    });

    component.saveMapping();

    expect(userMappingService.saveUserProjectMapping).toHaveBeenCalledWith("user-2", ["project-2"]);
    expect(component.userProjectMappings).toContainEqual({
      userId: "user-2",
      projectIdList: ["project-2"],
    });
    expect(component.showProjectMappingDialog).toBe(false);
    expect(toastService.success).toHaveBeenCalledWith(
      "Project mapping saved",
      "The selected user project access was updated.",
    );
  });

  it("should mark invalid project mapping forms as touched", () => {
    component.openProjectMappingDialog();

    component.saveMapping();

    expect(component.mappingForm.touched).toBe(true);
    expect(userMappingService.saveUserProjectMapping).not.toHaveBeenCalled();
  });

  it("should save role mappings", () => {
    userMappingService.assignUserRole!.mockReturnValue(
      of({ userId: "user-1", roleId: "role-1", userName: "Asha", roleName: "Admin" } as any),
    );
    component.openRoleMappingDialog();
    component.onRoleUserChange("user-1");
    component.onRoleChange("role-1");

    component.saveRoleMapping();

    expect(userMappingService.assignUserRole).toHaveBeenCalledWith("user-1", "role-1");
    expect(component.roleSuccessMessage).toBe("Asha mapped to Admin successfully.");
    expect(component.showRoleMappingDialog).toBe(false);
  });

  it("should show save errors and clear selected projects", () => {
    userMappingService.saveUserProjectMapping!.mockReturnValue(throwError(() => new Error("fail")));
    component.mappingForm.setValue({
      userId: "user-1",
      projectIds: ["project-1"],
    });

    component.saveMapping();
    component.clearProjects();

    expect(component.errorMessage).toBe("");
    expect(component.mappingForm.controls.projectIds.value).toEqual([]);
    expect(toastService.error).toHaveBeenCalledWith(
      "Unable to save mapping",
      "Unable to save the project mapping.",
    );
  });
});
