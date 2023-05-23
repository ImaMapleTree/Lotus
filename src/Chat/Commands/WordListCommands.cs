using System.Linq;
using HarmonyLib;
using Lotus.Managers;
using Lotus.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;

namespace Lotus.Chat.Commands;

[Command(CommandFlag.HostOnly, "wordlist", "wl")]
public class WordListCommands
{
    [Command("list")]
    private static void ListWords(PlayerControl source)
    {
        ChatHandler.Send(source, ChatManager.BannedWords.Select((w, i) => $"{i+1}) {w}").Join(delimiter: "\n"));
    }

    [Command("add")]
    private static void AddWord(PlayerControl source, CommandContext context, string word)
    {
        ChatManager.AddWord(word);
    }

    [Command("reload")]
    private static void Reload(PlayerControl source)
    {
        ChatManager.Reload();
        ChatHandler.Send(source, "Successfully Reloaded Wordlist");
    }

    private static ChatManager ChatManager => PluginDataManager.ChatManager;

}