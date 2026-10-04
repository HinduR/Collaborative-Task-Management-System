import { TestBed } from "@angular/core/testing";
import {
  HubConnectionState,
} from "@microsoft/signalr";

import { AuthService } from "../../../../service/auth/auth.service";
import { BoardRealtimeService } from "./board-realtime.service";

const connections: FakeHubConnection[] = [];
let failNextStart = false;

class FakeHubConnection {
  state = HubConnectionState.Disconnected;
  handlers = new Map<string, (...args: unknown[]) => void>();
  reconnectingHandler?: () => void;
  reconnectedHandler?: () => Promise<void>;
  closeHandler?: () => void;
  start = jest.fn(async () => {
    if (failNextStart) {
      failNextStart = false;
      throw new Error("start failed");
    }

    this.state = HubConnectionState.Connected;
  });
  stop = jest.fn(async () => {
    this.state = HubConnectionState.Disconnected;
  });
  invoke = jest.fn(async () => null);
  on(eventName: string, handler: (...args: unknown[]) => void): void {
    this.handlers.set(eventName, handler);
  }
  onreconnecting(handler: () => void): void {
    this.reconnectingHandler = handler;
  }
  onreconnected(handler: () => Promise<void>): void {
    this.reconnectedHandler = handler;
  }
  onclose(handler: () => void): void {
    this.closeHandler = handler;
  }
}

jest.mock("@microsoft/signalr", () => {
  const actual = jest.requireActual("@microsoft/signalr");

  return {
    ...actual,
    HubConnectionBuilder: jest.fn().mockImplementation(() => ({
      withUrl: jest.fn().mockReturnThis(),
      withAutomaticReconnect: jest.fn().mockReturnThis(),
      configureLogging: jest.fn().mockReturnThis(),
      build: jest.fn(() => {
        const connection = new FakeHubConnection();
        connections.push(connection);
        return connection;
      }),
    })),
  };
});

describe("BoardRealtimeService", () => {
  let service: BoardRealtimeService;
  let authService: {
    getAccessToken: jest.Mock;
  };

  beforeEach(() => {
    connections.length = 0;
    failNextStart = false;
    authService = {
      getAccessToken: jest.fn(() => "access-token"),
    };

    TestBed.configureTestingModule({
      providers: [
        BoardRealtimeService,
        {
          provide: AuthService,
          useValue: authService,
        },
      ],
    });

    service = TestBed.inject(BoardRealtimeService);
  });

  it("connects, joins the board, and reports connected state", async () => {
    await service.connect("board-1");

    expect(connections).toHaveLength(1);
    expect(connections[0].start).toHaveBeenCalled();
    expect(connections[0].invoke).toHaveBeenCalledWith("JoinBoard", "board-1");
    expect(service.connected()).toBe(true);
  });

  it("reuses an existing connection for the same board", async () => {
    await service.connect("board-1");
    await service.connect("board-1");

    expect(connections).toHaveLength(1);
  });

  it("emits task movement and de-duplicates active users", async () => {
    const taskEvents: unknown[] = [];
    service.taskMoved$.subscribe((event) => taskEvents.push(event));

    await service.connect("board-1");

    const moveEvent = {
      taskId: "task-1",
      workflowColumnId: "column-2",
    };

    connections[0].handlers.get("TaskMoved")?.(moveEvent);
    connections[0].handlers.get("BoardPresenceChanged")?.([
      {
        userId: "user-1",
        displayName: "First",
      },
      {
        userId: "user-1",
        displayName: "Latest",
      },
    ]);

    expect(taskEvents).toEqual([moveEvent]);
    expect(service.activeUsers()).toEqual([
      {
        userId: "user-1",
        displayName: "Latest",
      },
    ]);
  });

  it("clears state while reconnecting and reloads after reconnected", async () => {
    const reloads: number[] = [];
    service.boardReloadRequested$.subscribe(() => reloads.push(1));

    await service.connect("board-1");

    connections[0].reconnectingHandler?.();

    expect(service.connected()).toBe(false);
    expect(service.activeUsers()).toEqual([]);

    await connections[0].reconnectedHandler?.();

    expect(service.connected()).toBe(true);
    expect(connections[0].invoke).toHaveBeenCalledWith("JoinBoard", "board-1");
    expect(reloads).toEqual([1]);
  });

  it("disconnects from the board and clears state", async () => {
    await service.connect("board-1");

    await service.disconnect();

    expect(connections[0].invoke).toHaveBeenCalledWith("LeaveBoard", "board-1");
    expect(connections[0].stop).toHaveBeenCalled();
    expect(service.connected()).toBe(false);
    expect(service.activeUsers()).toEqual([]);
  });

  it("clears state when connecting fails", async () => {
    await service.connect("board-1");
    failNextStart = true;

    await expect(service.connect("board-2")).rejects.toThrow("start failed");

    expect(service.connected()).toBe(false);
    expect(service.activeUsers()).toEqual([]);
  });
});
