import { Component, inject } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { ToastComponent } from "./shared/component/toast/toast.component";
import { ThemeService } from "./shared/service/theme/theme.service";

@Component({
  selector: "app-root",
  imports: [RouterOutlet, ToastComponent],
  templateUrl: "./app.html",
  styleUrl: "./app.scss",
})
export class App {
  constructor() {
    inject(ThemeService);
  }
}
