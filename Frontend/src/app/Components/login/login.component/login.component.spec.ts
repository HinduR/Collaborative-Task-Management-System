import { ComponentFixture, TestBed } from "@angular/core/testing";

import {
  LOGIN_LOCATION,
  LoginComponent,
} from "./login.component";
import { environment } from "../../../../environment/environment";

describe("LoginComponent", () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let locationMock: {
    assign: jest.Mock;
  };

  beforeEach(async () => {
    locationMock = {
      assign: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        {
          provide: LOGIN_LOCATION,
          useValue: locationMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("does not redirect again while a redirect is already in progress", () => {
    component.isRedirecting = true;

    component.loginWithGoogle();

    expect(component.isRedirecting).toBe(true);
    expect(locationMock.assign).not.toHaveBeenCalled();
  });

  it("redirects to the Google login endpoint", () => {
    component.loginWithGoogle();

    expect(component.isRedirecting).toBe(true);
    expect(locationMock.assign).toHaveBeenCalledWith(
      `${environment.gatewayUrl}/api/identity/auth/google/login`,
    );
  });
});
