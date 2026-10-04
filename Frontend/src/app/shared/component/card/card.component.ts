import {
  ChangeDetectionStrategy,
  Component,
  Input,
} from '@angular/core';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CardModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() styleClass = '';

  // Pass the task type name here.
  @Input() type = '';

  get cardStyleClass(): string {
    const typeClass = this.getTypeClass();

    return [
      'shared-card',
      typeClass,
      this.styleClass,
    ]
      .filter(Boolean)
      .join(' ');
  }

  private getTypeClass(): string {
    switch (this.type.trim().toLowerCase()) {
      case 'bug':
        return 'card-type-bug';

      case 'feature':
        return 'card-type-feature';

      case 'story':
        return 'card-type-story';

      case 'task':
        return 'card-type-task';

      default:
        return 'card-type-default';
    }
  }
}