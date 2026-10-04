import { ChangeDetectionStrategy, Component, DestroyRef, Input, inject, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ButtonComponent } from '../../shared/component/button/button.component';
import { SidebarItem } from '../model/side-bar';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [ButtonComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  @Input() collapsed = false;
  @Input() isSystemAdmin = false;

  readonly currentUrl = signal(this.router.url);

  readonly navigationItems: SidebarItem[] = [
    {
      id: 'projects',
      label: 'Projects',
      icon: 'pi pi-th-large',
      route: '/projects',
    },
    {
      id: 'user-mapping',
      label: 'User Mapping',
      icon: 'pi pi-user-edit',
      route: '/admin/user-mapping',
      requiredRole: 'Admin',
    },
  ];

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((event) => {
        this.currentUrl.set(event.urlAfterRedirects);
      });
  }

  canDisplay(item: SidebarItem): boolean {
    return !item.requiredRole || this.isSystemAdmin;
  }

  navigateTo(item: SidebarItem): void {
    void this.router.navigateByUrl(item.route);
  }

  isActive(item: SidebarItem): boolean {
    const url = this.currentUrl();

    return url === item.route || url.startsWith(`${item.route}/`);
  }
}
