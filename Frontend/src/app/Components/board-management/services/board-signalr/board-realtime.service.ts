import {
  inject,
  Injectable,
  signal,
} from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import {
  Subject,
} from 'rxjs';

import { environment } from '../../../../../environment/environment';
import { BoardActiveUser } from '../../models/board-active-user';
import { TaskMovedEvent } from '../../models/task-moved-event.model';
import { AuthService } from '../../../../service/auth/auth.service';

@Injectable({
  providedIn: 'root',
})
export class BoardRealtimeService {
  private readonly authService =
    inject(AuthService);

  private hubConnection?: HubConnection;
  private currentBoardId?: string;
  private connectionPromise?: Promise<void>;

  private readonly boardHubUrl =
    `${environment.gatewayUrl}` +
    `${environment.endpoints.boardTask}` +
    '/hubs/board';

  private readonly taskMovedSubject =
    new Subject<TaskMovedEvent>();

  readonly taskMoved$ =
    this.taskMovedSubject.asObservable();

  private readonly boardReloadSubject =
    new Subject<void>();

  readonly boardReloadRequested$ =
    this.boardReloadSubject.asObservable();

  private readonly activeUsersSignal =
    signal<BoardActiveUser[]>([]);

  readonly activeUsers =
    this.activeUsersSignal.asReadonly();

  private readonly connectedSignal =
    signal(false);

  readonly connected =
    this.connectedSignal.asReadonly();

  connect(boardId: string): Promise<void> {
    if (
      this.hubConnection?.state ===
        HubConnectionState.Connected &&
      this.currentBoardId === boardId
    ) {
      return Promise.resolve();
    }

    if (
      this.connectionPromise &&
      this.currentBoardId === boardId
    ) {
      return this.connectionPromise;
    }

    this.connectionPromise =
      this.createConnection(boardId);

    return this.connectionPromise.finally(() => {
      this.connectionPromise = undefined;
    });
  }

  private async createConnection(
    boardId: string,
  ): Promise<void> {
    await this.disconnect();

    this.currentBoardId = boardId;

    const connection =
      new HubConnectionBuilder()
        .withUrl(this.boardHubUrl, {
          accessTokenFactory: () =>
            this.authService.getAccessToken() ?? '',
        })
        .withAutomaticReconnect([
          0,
          2000,
          5000,
          10000,
        ])
        .configureLogging(
          LogLevel.None,
        )
        .build();

    this.hubConnection = connection;

    this.registerEvents(connection);
    this.registerConnectionEvents(connection);

    try {
      await connection.start();

      if (this.hubConnection !== connection) {
        await connection.stop();
        return;
      }

      this.connectedSignal.set(true);

      await this.joinBoard(boardId);
    } catch (error) {
      this.connectedSignal.set(false);
      this.activeUsersSignal.set([]);

      if (this.hubConnection === connection) {
        this.hubConnection = undefined;
        this.currentBoardId = undefined;
      }

      try {
        await connection.stop();
      } catch {
        // Ignore cleanup errors.
      }

      throw error;
    }
  }

  async disconnect(): Promise<void> {
    const connection = this.hubConnection;
    const boardId = this.currentBoardId;

    if (!connection) {
      this.currentBoardId = undefined;
      this.clearConnectionState();
      return;
    }

    try {
      if (
        boardId &&
        connection.state ===
          HubConnectionState.Connected
      ) {
        await connection.invoke(
          'LeaveBoard',
          boardId,
        );
      }
    } catch (error) {
      console.error(
        'Unable to leave the SignalR board group.',
        error,
      );
    } finally {
      try {
        await connection.stop();
      } catch (error) {
        console.error(
          'Unable to stop the SignalR connection.',
          error,
        );
      } finally {
        if (this.hubConnection === connection) {
          this.hubConnection = undefined;
          this.currentBoardId = undefined;
        }

        this.clearConnectionState();
      }
    }
  }

  private registerEvents(
    connection: HubConnection,
  ): void {
    connection.on(
      'TaskMoved',
      (event: TaskMovedEvent) => {
        this.taskMovedSubject.next(event);
      },
    );

    connection.on(
      'BoardPresenceChanged',
      (users: BoardActiveUser[]) => {
        this.activeUsersSignal.set(
          this.removeDuplicateUsers(
            users ?? [],
          ),
        );
      },
    );
  }

  private registerConnectionEvents(
    connection: HubConnection,
  ): void {
    connection.onreconnecting(() => {
      if (this.hubConnection !== connection) {
        return;
      }

      this.connectedSignal.set(false);
      this.activeUsersSignal.set([]);
    });

    connection.onreconnected(async () => {
      if (this.hubConnection !== connection) {
        return;
      }

      this.connectedSignal.set(true);

      const boardId = this.currentBoardId;

      if (!boardId) {
        return;
      }

      try {
        await connection.invoke(
          'JoinBoard',
          boardId,
        );

        /*
         * Events may have been missed while disconnected.
         * BoardViewComponent should reload tasks after this.
         */
        this.boardReloadSubject.next();
      } catch {
        this.connectedSignal.set(false);
      }
    });

    connection.onclose(() => {
      if (this.hubConnection !== connection) {
        return;
      }

      this.connectedSignal.set(false);
      this.activeUsersSignal.set([]);
    });
  }

  private async joinBoard(
    boardId: string,
  ): Promise<void> {
    const connection = this.hubConnection;

    if (
      !connection ||
      connection.state !==
        HubConnectionState.Connected
    ) {
      return;
    }

    await connection.invoke(
      'JoinBoard',
      boardId,
    );
  }

  private removeDuplicateUsers(
    users: BoardActiveUser[],
  ): BoardActiveUser[] {
    return Array.from(
      new Map(
        users.map((user) => [
          user.userId,
          user,
        ]),
      ).values(),
    );
  }

  private clearConnectionState(): void {
    this.connectedSignal.set(false);
    this.activeUsersSignal.set([]);
  }
}