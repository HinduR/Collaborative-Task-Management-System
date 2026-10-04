import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
} from "@angular/core";
import { ButtonModule } from "primeng/button";

export type ButtonType = "button" | "submit" | "reset";
export type IconPosition = "left" | "right";

@Component({
  selector: "app-button",
  standalone: true,
  imports: [ButtonModule],
  templateUrl: "./button.component.html",
  styleUrl: "./button.component.scss",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ButtonComponent {
  @Input({ required: true }) buttonId!: string;
  @Input() label = '';

  @Input() type: ButtonType = 'button';
  @Input() icon = '';
  @Input() iconPosition: IconPosition = 'left';

  @Input() disabled = false;
  @Input() loading = false;
  @Input() fullWidth = false;

  @Input() styleClass = '';
  @Input() ariaLabel = '';
  @Input() title = '';

  @Output() buttonClick = new EventEmitter<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (this.disabled || this.loading) {
      return;
    }

    this.buttonClick.emit(event);
  }
}
