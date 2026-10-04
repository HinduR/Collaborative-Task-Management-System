import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';

import { AuthService } from './auth.service';
import { environment } from '../../../environment/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpTestingController: HttpTestingController;
  const identityUrl = `${environment.gatewayUrl}/api/identity`;

  beforeEach(() => {
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('exchanges a Google code for an access token', () => {
    const response = { accessToken: 'token-1' };

    service.exchangeGoogleCode('auth-code').subscribe((result) => {
      expect(result).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${identityUrl}/auth/google/exchange`,
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ code: 'auth-code' });
    expect(request.request.withCredentials).toBe(true);

    request.flush(response);
  });

  it('refreshes the access token with credentials', () => {
    const response = { accessToken: 'fresh-token' };

    service.refreshAccessToken().subscribe((result) => {
      expect(result).toEqual(response);
    });

    const request = httpTestingController.expectOne(
      `${identityUrl}/auth/refresh`,
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({});
    expect(request.request.withCredentials).toBe(true);

    request.flush(response);
  });

  it('stores, reads, clears, and checks access tokens', () => {
    expect(service.getAccessToken()).toBeNull();
    expect(service.isLoggedIn()).toBe(false);

    service.setAccessToken('stored-token');

    expect(service.getAccessToken()).toBe('stored-token');
    expect(service.isLoggedIn()).toBe(true);

    service.clearAccessToken();

    expect(service.getAccessToken()).toBeNull();
    expect(service.isLoggedIn()).toBe(false);
  });

  it('loads and stores the current user when no user id is supplied', () => {
    const user = {
      userId: 'user-1',
      name: 'Ada',
      email: 'ada@example.com',
    };

    service.getUserDetails().subscribe((result) => {
      expect(result).toEqual(user);
    });

    const request = httpTestingController.expectOne(
      `${identityUrl}/user/details`,
    );

    expect(request.request.method).toBe('GET');
    expect(request.request.params.keys()).toEqual([]);

    request.flush(user);

    expect(service.currentUser()).toEqual(user);
  });

  it('loads another user without replacing the current user', () => {
    const currentUser = {
      userId: 'current-user',
      name: 'Current',
      email: 'current@example.com',
    };
    const otherUser = {
      userId: 'other-user',
      name: 'Other',
      email: 'other@example.com',
    };

    service.getUserDetails().subscribe();
    httpTestingController
      .expectOne(`${identityUrl}/user/details`)
      .flush(currentUser);

    service.getUserDetails('other-user').subscribe((result) => {
      expect(result).toEqual(otherUser);
    });

    const request = httpTestingController.expectOne(
      (req) =>
        req.url === `${identityUrl}/user/details` &&
        req.params.get('userId') === 'other-user',
    );

    request.flush(otherUser);

    expect(service.currentUser()).toEqual(currentUser);
  });

  it('clears authentication after logout succeeds', () => {
    const user = {
      userId: 'user-1',
      name: 'Ada',
      email: 'ada@example.com',
    };

    service.setAccessToken('stored-token');
    service.getUserDetails().subscribe();
    httpTestingController
      .expectOne(`${identityUrl}/user/details`)
      .flush(user);

    service.logout().subscribe((result) => {
      expect(result).toBeNull();
    });

    const request = httpTestingController.expectOne(
      `${identityUrl}/auth/logout`,
    );

    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBe(true);

    request.flush(null);

    expect(service.getAccessToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
  });
});
