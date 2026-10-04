namespace Shared.SignalR.Constants;

public static class SignalRConstants
{
    public const string AccessTokenQueryParameter = "access_token";

    public const string HubPathPrefix = "/hubs";

    public const string BoardHubPath = "/hubs/board";

    public const string BoardGroupPrefix = "board";

    public const string TaskMovedEvent = "TaskMoved";

    public const string UserJoinedEvent = "UserJoined";

    public const string UserLeftEvent = "UserLeft";
}