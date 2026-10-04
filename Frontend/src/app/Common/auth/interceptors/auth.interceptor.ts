import {
  HttpErrorResponse,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  catchError,
  switchMap,
  throwError,
} from 'rxjs';
import { environment } from '../../../../environment/environment';
import { TokenRefreshService } from '../service/token-refresh.service';
import { AuthService } from '../../../service/auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (
  request,
  next,
) => {
  const authService = inject(AuthService);
  const tokenRefreshService =
    inject(TokenRefreshService);
  const router = inject(Router);

  if (
    !request.url.startsWith(
      environment.gatewayUrl,
    )
  ) {
    return next(request);
  }

  const publicAuthenticationRequest =
    isPublicAuthenticationRequest(
      request.url,
    );

  const requestToSend =
    publicAuthenticationRequest
      ? request.clone({
          withCredentials: true,
        })
      : addAccessToken(
          request,
          authService.getAccessToken(),
        );

  return next(requestToSend).pipe(
    catchError(
      (error: HttpErrorResponse) => {
        if (
          error.status !== 401 ||
          publicAuthenticationRequest
        ) {
          return throwError(() => error);
        }

        return tokenRefreshService
          .refreshSession()
          .pipe(
            switchMap((response) => {
              const retryRequest =
                addAccessToken(
                  request,
                  response.accessToken,
                );

              return next(retryRequest);
            }),
            catchError((refreshError) => {
              authService.clearAuthentication();

              void router.navigate(
                ['/login'],
                {
                  replaceUrl: true,
                },
              );

              return throwError(
                () => refreshError,
              );
            }),
          );
      },
    ),
  );
};

function addAccessToken(
  request: HttpRequest<unknown>,
  accessToken: string | null,
): HttpRequest<unknown> {
  if (!accessToken) {
    return request;
  }

  return request.clone({
    setHeaders: {
      Authorization:
        `Bearer ${accessToken}`,
    },
  });
}

function isPublicAuthenticationRequest(
  url: string,
): boolean {
  return (
    url.includes(
      '/identity/auth/google/login',
    ) ||
    url.includes(
      '/identity/auth/google/exchange',
    ) ||
    url.endsWith(
      '/identity/auth/refresh',
    ) ||
    url.endsWith(
      '/identity/auth/logout',
    )
  );
}