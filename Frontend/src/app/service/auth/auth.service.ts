import {
  HttpClient,
  HttpParams,
} from '@angular/common/http';
import {
  inject,
  Injectable,
  signal,
} from '@angular/core';
import {
  Observable,
  tap,
} from 'rxjs';
import { environment } from '../../../environment/environment';
import {
  AccessTokenResponse,
  GoogleLoginCodeExchangeRequest,
} from '../../Common/auth/models/auth-response';
import { AuthenticatedUser } from '../../Common/auth/models/authenticated-user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly httpClient = inject(HttpClient);

  private readonly accessTokenKey = 'accessToken';

  private readonly identityUrl =
    `${environment.gatewayUrl}/api/identity`;

  private readonly currentUserSignal =
    signal<AuthenticatedUser | null>(null);

  readonly currentUser =
    this.currentUserSignal.asReadonly();


  exchangeGoogleCode(
    code: string,
  ): Observable<AccessTokenResponse> {
    const request: GoogleLoginCodeExchangeRequest = {
      code,
    };

    return this.httpClient.post<AccessTokenResponse>(
      `${this.identityUrl}/auth/google/exchange`,
      request,
      {
        withCredentials: true,
      },
    );
  }

  refreshAccessToken(): Observable<AccessTokenResponse> {
    return this.httpClient.post<AccessTokenResponse>(
      `${this.identityUrl}/auth/refresh`,
      {},
      {
        withCredentials: true,
      },
    );
  }

  logout(): Observable<void> {
    return this.httpClient.post<void>(
      `${this.identityUrl}/auth/logout`,
      {},
      {
        withCredentials: true,
      },
    ).pipe(
      tap(() => {
        this.clearAuthentication();
      }),
    );
  }

  setAccessToken(accessToken: string): void {
    sessionStorage.setItem(
      this.accessTokenKey,
      accessToken,
    );
  }

  getAccessToken(): string | null {
    return sessionStorage.getItem(
      this.accessTokenKey,
    );
  }

  clearAccessToken(): void {
    sessionStorage.removeItem(
      this.accessTokenKey,
    );
  }

  isLoggedIn(): boolean {
    return Boolean(this.getAccessToken());
  }

  getUserDetails(
    userId?: string,
  ): Observable<AuthenticatedUser> {
    let params = new HttpParams();

    if (userId) {
      params = params.set(
        'userId',
        userId,
      );
    }

    return this.httpClient
      .get<AuthenticatedUser>(
        `${this.identityUrl}/user/details`,
        {
          params,
        },
      )
      .pipe(
        tap((user) => {
          if (!userId) {
            this.currentUserSignal.set(user);
          }
        }),
      );
  }

  clearAuthentication(): void {
    this.clearAccessToken();
    this.currentUserSignal.set(null);
  }
}