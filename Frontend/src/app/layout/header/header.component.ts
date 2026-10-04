import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
} from "@angular/core";
import { RouterLink } from "@angular/router";

import { ButtonComponent } from "../../shared/component/button/button.component";
import { ThemeService } from "../../shared/service/theme/theme.service";

@Component({
  selector: "app-header",
  standalone: true,
  imports: [ButtonComponent, RouterLink],
  templateUrl: "./header.component.html",
  styleUrl: "./header.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  private readonly themeService = inject(ThemeService);

  @Input() applicationName = "The Round Table";
  @Input() userName = "User";

  @Output() sidebarToggle = new EventEmitter<void>();
  @Output() logout = new EventEmitter<void>();

  readonly isDarkMode = this.themeService.isDarkMode;

  get themeToggleIcon(): string {
    return this.isDarkMode() ? "pi pi-sun" : "pi pi-moon";
  }

  get themeToggleLabel(): string {
    return this.isDarkMode()
      ? "Switch to light mode"
      : "Switch to dark mode";
  }

  onSidebarToggle(): void {
    this.sidebarToggle.emit();
  }

  onThemeToggle(): void {
    this.themeService.toggleTheme();
  }

  onLogout(): void {
    this.logout.emit();
  }
}
