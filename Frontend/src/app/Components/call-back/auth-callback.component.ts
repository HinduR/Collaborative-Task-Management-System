import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../service/auth/auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  template: `
    <div class="auth-callback">
      <p>Completing Google login...</p>
    </div>
  `,
  styleUrl: './auth-callback.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  private exchangeStarted = false;

  ngOnInit(): void {
    if (this.exchangeStarted) {
      return;
    }

    this.exchangeStarted = true;

    const code =
      this.route.snapshot.queryParamMap.get('code');

    if (!code) {
      void this.navigateToLogin(
        'missing-login-code',
      );

      return;
    }

  this.authService.exchangeGoogleCode(code).subscribe({
  next: (response) => {
    this.authService.setAccessToken(
      response.accessToken,
    );

    void this.router.navigate(
      ['/projects'],
      {
        replaceUrl: true,
      },
    );
  },
  error: () => {
    this.authService.clearAuthentication();

    void this.router.navigate(
      ['/login'],
      {
        replaceUrl: true,
      },
    );
  },
});
  }

  private navigateToLogin(
    error: string,
  ): Promise<boolean> {
    return this.router.navigate(
      ['/login'],
      {
        queryParams: {
          error,
        },
        replaceUrl: true,
      },
    );
  }
}