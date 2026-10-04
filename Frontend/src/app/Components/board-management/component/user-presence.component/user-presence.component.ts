import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';
import { BoardActiveUser } from '../../models/board-active-user';


@Component({
  selector: 'app-user-presence',
  standalone: true,
  templateUrl: './user-presence.component.html',
  styleUrl: './user-presence.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserPresenceComponent {
  readonly users =
    input<BoardActiveUser[]>([]);

  getInitial(userName: string): string {
    const normalizedName = userName.trim();

    return normalizedName
      ? normalizedName.charAt(0).toUpperCase()
      : '?';
  }

  getRemainingUserNames(
    users: BoardActiveUser[],
  ): string {
    return users
      .slice(4)
      .map((user) => user.userName)
      .join(', ');
  }
}