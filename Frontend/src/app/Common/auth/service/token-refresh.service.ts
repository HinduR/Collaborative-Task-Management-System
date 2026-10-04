import {
  inject,
  Injectable,
} from '@angular/core';
import {
  finalize,
  Observable,
  shareReplay,
  tap,
} from 'rxjs';
import { AuthService } from '../../../service/auth/auth.service';
import { AccessTokenResponse } from '../models/auth-response';


@Injectable({
  providedIn: 'root',
})
export class TokenRefreshService {
  private readonly authService = inject(AuthService);

  private refreshRequest:
    Observable<AccessTokenResponse> | null = null;

  refreshSession(): Observable<AccessTokenResponse> {
    if (this.refreshRequest) {
      return this.refreshRequest;
    }

    this.refreshRequest =
      this.authService.refreshAccessToken().pipe(
        tap((response) => {
          this.authService.setAccessToken(
            response.accessToken,
          );
        }),
        finalize(() => {
          this.refreshRequest = null;
        }),
        shareReplay({
          bufferSize: 1,
          refCount: false,
        }),
      );

    return this.refreshRequest;
  }
}