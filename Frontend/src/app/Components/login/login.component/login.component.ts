import {
  ChangeDetectionStrategy,
  Component,
  InjectionToken,
  inject,
} from "@angular/core";
import { environment } from "../../../../environment/environment";
import { ButtonComponent } from "../../../shared/component/button/button.component";

export const LOGIN_LOCATION = new InjectionToken<Location>("LOGIN_LOCATION", {
  providedIn: "root",
  factory: () => window.location,
});

@Component({
  selector: "app-login",
  standalone: true,
  imports: [ButtonComponent],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly location = inject(LOGIN_LOCATION);

  isRedirecting = false;

  loginWithGoogle(): void {
    if (this.isRedirecting) {
      return;
    }

    this.isRedirecting = true;

    this.location.assign(
      `${environment.gatewayUrl}` + "/api/identity/auth/google/login",
    );
  }
}
