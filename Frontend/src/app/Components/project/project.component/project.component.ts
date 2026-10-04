import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Router } from "@angular/router";
import { finalize } from "rxjs";

import { CardComponent } from "../../../shared/component/card/card.component";
import { ToastService } from "../../../shared/service/toast/toast.service";
import { ProjectSummary } from "../../user-mapping/model/project-mapping";
import { UserMappingService } from "../../user-mapping/service/user-mapping-service";

@Component({
  selector: "app-project",
  standalone: true,
  imports: [CardComponent],
  templateUrl: "./project.component.html",
  styleUrl: "./project.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectComponent implements OnInit {
  private readonly userMappingService = inject(UserMappingService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  projects: ProjectSummary[] = [];
  isLoading = false;

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.isLoading = true;

    this.userMappingService
      .getProjectList()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (projects) => {
          this.projects = projects ?? [];
        },
        error: () => {
          this.projects = [];
          this.toastService.error(
            "Unable to load projects",
            "Please try again later.",
          );
        },
      });
  }

  openProject(project: ProjectSummary): void {
    if (!project.isAssigned) {
      this.toastService.warning(
        "Access denied",
        "You are not assigned to this project.",
      );

      return;
    }

    void this.router.navigate(["/projects", project.id, "boards"]);
  }

  handleKeyDown(event: KeyboardEvent, project: ProjectSummary): void {
    if (event.key !== "Enter" && event.key !== " ") {
      return;
    }

    event.preventDefault();
    this.openProject(project);
  }
}
