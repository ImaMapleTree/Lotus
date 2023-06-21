using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;

namespace Lotus.Chat.Commands;

[Command("history")]
public class HistoryCommands : ICommandReceiver
{
    public void Receive(PlayerControl source, CommandContext context)
    {
        if (Game.MatchData.GameHistory == null!) return;
        ChatHandler.Send(source, Game.MatchData.GameHistory.Events.Where(e => e.IsCompletion()).Select(e => e.GenerateMessage()).Join(delimiter: "\n"));
    }
}