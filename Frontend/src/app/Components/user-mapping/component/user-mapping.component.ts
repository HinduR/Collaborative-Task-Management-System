import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  inject,
} from "@angular/core";
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { finalize, forkJoin } from "rxjs";
import { DropdownOption } from "../../../shared/models/dropdownOption";

import { UserProjectMapping } from "../model/user-project-mapping";
import { UserMappingService } from "../service/user-mapping-service";
import { ButtonComponent } from "../../../shared/component/button/button.component";
import { MultiSelectComponent } from "../../../shared/component/multi-select/multi-select.component";
import { DropdownComponent } from "../../../shared/component/dropdown/dropdown.component";
import { DialogComponent } from "../../../shared/component/dialog/dialog.component";
import { ToastService } from "../../../shared/service/toast/toast.service";

@Component({
  selector: "app-user-mapping",
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    DialogComponent,
    DropdownComponent,
    MultiSelectComponent,
  ],
  templateUrl: "./user-mapping.component.html",
  styleUrl: "./user-mapping.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserMappingComponent implements OnInit {
  private readonly userMappingService = inject(UserMappingService);

  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly toastService = inject(ToastService);

  users: DropdownOption[] = [];
  projects: DropdownOption[] = [];
  roles: DropdownOption[] = [];

  userProjectMappings: UserProjectMapping[] = [];

  showProjectMappingDialog = false;
  showRoleMappingDialog = false;

  isLoading = false;
  isSaving = false;
  isRoleSaving = false;

  successMessage = "";
  errorMessage = "";

  roleSuccessMessage = "";
  roleErrorMessage = "";

  readonly mappingForm = new FormGroup({
    userId: new FormControl<string | null>(null, Validators.required),
    projectIds: new FormControl<string[]>([], {
      nonNullable: true,
    }),
  });

  readonly roleMappingForm = new FormGroup({
    userId: new FormControl<string | null>(null, Validators.required),
    roleId: new FormControl<string | null>(null, Validators.required),
  });

  ngOnInit(): void {
    this.loadInitialData();
  }

  private loadInitialData(): void {
    this.isLoading = true;
    this.errorMessage = "";

    forkJoin({
      users: this.userMappingService.getUsers(),
      projects: this.userMappingService.getProjects(),
      roles: this.userMappingService.getRoles(),
      mappings: this.userMappingService.getUserProjectMappings(),
    })
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          this.users = response.users;
          this.projects = response.projects;
          this.roles = response.roles;
          this.userProjectMappings = response.mappings;
        },
        error: () => {
          this.errorMessage = "Unable to load user mapping details.";
          this.toastService.error("Unable to load mappings", this.errorMessage);
        },
      });
  }

  openProjectMappingDialog(): void {
    this.mappingForm.reset({
      userId: null,
      projectIds: [],
    });

    this.successMessage = "";
    this.errorMessage = "";
    this.showProjectMappingDialog = true;
  }

  onProjectDialogVisibilityChange(visible: boolean): void {
    if (!visible && this.isSaving) {
      return;
    }

    this.showProjectMappingDialog = visible;
  }

  closeProjectMappingDialog(): void {
    if (!this.isSaving) {
      this.showProjectMappingDialog = false;
    }
  }

  openRoleMappingDialog(): void {
    this.roleMappingForm.reset({
      userId: null,
      roleId: null,
    });

    this.roleSuccessMessage = "";
    this.roleErrorMessage = "";
    this.showRoleMappingDialog = true;
  }

  onRoleDialogVisibilityChange(visible: boolean): void {
    if (!visible && this.isRoleSaving) {
      return;
    }

    this.showRoleMappingDialog = visible;
  }

  closeRoleMappingDialog(): void {
    if (!this.isRoleSaving) {
      this.showRoleMappingDialog = false;
    }
  }

  onProjectUserChange(userId: string | null): void {
    this.mappingForm.controls.userId.setValue(userId);
    this.mappingForm.controls.userId.markAsTouched();

    this.successMessage = "";
    this.errorMessage = "";

    if (!userId) {
      this.mappingForm.controls.projectIds.setValue([]);
      return;
    }

    const existingMapping = this.userProjectMappings.find(
      (mapping) => mapping.userId.toLowerCase() === userId.toLowerCase(),
    );

    this.mappingForm.controls.projectIds.setValue(
      existingMapping?.projectIdList ?? [],
    );
  }

  onRoleUserChange(userId: string | null): void {
    this.roleMappingForm.controls.userId.setValue(userId);
    this.roleMappingForm.controls.userId.markAsTouched();

    this.roleSuccessMessage = "";
    this.roleErrorMessage = "";
  }

  onRoleChange(roleId: string | null): void {
    this.roleMappingForm.controls.roleId.setValue(roleId);
    this.roleMappingForm.controls.roleId.markAsTouched();

    this.roleSuccessMessage = "";
    this.roleErrorMessage = "";
  }

  saveMapping(): void {
    if (this.mappingForm.invalid || this.isSaving) {
      this.mappingForm.markAllAsTouched();
      return;
    }

    const userId = this.mappingForm.controls.userId.value;

    if (!userId) {
      return;
    }

    const projectIds = this.mappingForm.controls.projectIds.getRawValue();

    this.isSaving = true;
    this.successMessage = "";
    this.errorMessage = "";

    this.userMappingService
      .saveUserProjectMapping(userId, projectIds)
      .pipe(
        finalize(() => {
          this.isSaving = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          this.updateLocalProjectMapping(userId, projectIds);

          this.successMessage = "Project mapping saved successfully.";
          this.showProjectMappingDialog = false;
          this.mappingForm.reset({
            userId: null,
            projectIds: [],
          });

          this.toastService.success(
            "Project mapping saved",
            "The selected user project access was updated.",
          );
        },
        error: () => {
          this.errorMessage = "Unable to save the project mapping.";
          this.toastService.error("Unable to save mapping", this.errorMessage);
        },
      });
  }

  saveRoleMapping(): void {
    if (this.roleMappingForm.invalid || this.isRoleSaving) {
      this.roleMappingForm.markAllAsTouched();
      return;
    }

    const { userId, roleId } = this.roleMappingForm.getRawValue();

    if (!userId || !roleId) {
      return;
    }

    this.isRoleSaving = true;
    this.roleSuccessMessage = "";
    this.roleErrorMessage = "";

    this.userMappingService
      .assignUserRole(userId, roleId)
      .pipe(
        finalize(() => {
          this.isRoleSaving = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (response) => {
          this.roleSuccessMessage =
            `${response.userName} mapped to ` +
            `${response.roleName} successfully.`;
          this.showRoleMappingDialog = false;
          this.roleMappingForm.reset({
            userId: null,
            roleId: null,
          });

          this.toastService.success(
            "Role mapping saved",
            this.roleSuccessMessage,
          );
        },
        error: () => {
          this.roleErrorMessage = "Unable to save the role mapping.";
          this.toastService.error("Unable to save role", this.roleErrorMessage);
        },
      });
  }

  clearProjects(): void {
    this.mappingForm.controls.projectIds.setValue([]);
    this.mappingForm.controls.projectIds.markAsDirty();

    this.successMessage = "";
    this.errorMessage = "";
  }

  private updateLocalProjectMapping(
    userId: string,
    projectIds: string[],
  ): void {
    const updatedMapping: UserProjectMapping = {
      userId,
      projectIdList: [...projectIds],
    };

    const existingIndex = this.userProjectMappings.findIndex(
      (mapping) => mapping.userId.toLowerCase() === userId.toLowerCase(),
    );

    if (existingIndex < 0) {
      this.userProjectMappings = [...this.userProjectMappings, updatedMapping];

      return;
    }

    this.userProjectMappings = this.userProjectMappings.map((mapping, index) =>
      index === existingIndex ? updatedMapping : mapping,
    );
  }
}
