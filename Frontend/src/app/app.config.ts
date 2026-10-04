import Aura from "@primeng/themes/aura";
import { definePreset } from "@primeng/themes";
import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from "@angular/core";
import { provideRouter } from "@angular/router";
import { routes } from "./app.routes";
import { providePrimeNG } from "primeng/config";
import { provideAnimationsAsync } from "@angular/platform-browser/animations/async";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { authInterceptor } from "./Common/auth/interceptors/auth.interceptor";
import { MessageService } from "primeng/api";

const RoundTablePreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: "{violet.50}",
      100: "{violet.100}",
      200: "{violet.200}",
      300: "{violet.300}",
      400: "{violet.400}",
      500: "{violet.600}",
      600: "{violet.700}",
      700: "{violet.800}",
      800: "{violet.900}",
      900: "{violet.950}",
      950: "{violet.950}",
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideAnimationsAsync(),
    provideRouter(routes),
    providePrimeNG({
      ripple: true,
      theme: {
        preset: RoundTablePreset,
        options: {
          darkModeSelector: ".app-dark",
        },
      },
    }),
    provideHttpClient(withInterceptors([authInterceptor])),
    MessageService,
  ],
};
