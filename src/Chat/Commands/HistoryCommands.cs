using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.Utilities;
using Lotus.API;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;

namespace Lotus.Chat.Commands;

[Command("history")]
public class HistoryCommands : ICommandReceiver
{
    public bool Receive(PlayerControl source, CommandContext context)
    {
        if (Game.MatchData.GameHistory == null!) return true;
        Utils.SendMessage(Game.MatchData.GameHistory.Events.Where(e => e.IsCompletion()).Select(e => e.GenerateMessage()).Join(delimiter: "\n"), source.PlayerId);
        return true;
    }
}