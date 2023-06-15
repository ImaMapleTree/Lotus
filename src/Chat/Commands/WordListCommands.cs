using Lotus.Managers;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Utilities;

namespace Lotus.Chat.Commands;

[Command(CommandFlag.HostOnly, "wordlist", "wl")]
public class WordListCommands: CommandTranslations
{
    [Command("reload")]
    private static void Reload(PlayerControl source)
    {
        string? exception = PluginDataManager.ChatManager.Reload();
        if (exception != null) ErrorMsg(exception).Send(source);
        else SuccessMsg("Success Reloading").Send(source);
    }

    private static ChatManager ChatManager => PluginDataManager.ChatManager;

    private static ChatHandler SuccessMsg(string message) => ChatHandler.Of(message, title: Color.green.Colorize("Success")).LeftAlign();

    private static ChatHandler ErrorMsg(string message) => ChatHandler.Of(message, title: CommandError).LeftAlign();
}