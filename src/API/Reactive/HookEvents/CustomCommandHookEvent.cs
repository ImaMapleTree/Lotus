namespace Lotus.API.Reactive.HookEvents;

public class CustomCommandHookEvent: PlayerHookEvent
{
    public string Command;
    public string[] Args;

    public CustomCommandHookEvent(PlayerControl source, string command, string[] args) : base(source)
    {
        Command = command;
        Args = args;
    }
}