import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Router, RouterOutlet } from "@angular/router";
import { finalize } from "rxjs";

import { AuthService } from "../../service/auth/auth.service";
import { ToastService } from "../../shared/service/toast/toast.service";
import { HeaderComponent } from "../header/header.component";
import { SidebarComponent } from "../sidebar/sidebar.component";

@Component({
  selector: "app-main-layout",
  standalone: true,
  imports: [
    RouterOutlet,
    HeaderComponent,
    SidebarComponent,
  ],
  templateUrl: "./main-layout.component.html",
  styleUrl: "./main-layout.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MainLayoutComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly changeDetectorRef =
    inject(ChangeDetectorRef);
  private readonly toastService  = inject(ToastService);

  readonly applicationName = "Round Table";

  userName = "";
  isSystemAdmin = false;

  isLoadingUser = false;
  isSidebarCollapsed = false;
  isLoggingOut = false;

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  onSidebarToggle(): void {
    this.isSidebarCollapsed =
      !this.isSidebarCollapsed;
  }

  onLogout(): void {
    if (this.isLoggingOut) {
      return;
    }

    this.isLoggingOut = true;

    this.authService
      .logout()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoggingOut = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          void this.router.navigateByUrl("/login");
        },
        error: () => {
          this.toastService.error(
            "Logout failed",
            "Unable to log out. Please try again.",
          );
        },
      });
  }

  private loadCurrentUser(): void {
    this.isLoadingUser = true;

    this.authService
      .getUserDetails()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoadingUser = false;
          this.changeDetectorRef.markForCheck();
        }),
      )
      .subscribe({
        next: (user) => {
          this.userName =
            user.userName || user.email;

          this.isSystemAdmin = user.roles.some(
            (role) =>
              role.toLowerCase() ===
              "admin",
          );
        },
        error: () => {
          this.userName = "User";

          this.toastService.error(
            "User details unavailable",
            "Unable to load your user details.",
          );
        },
      });
  }
}