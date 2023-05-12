using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;

namespace TOHTOR.Chat.Commands;

[Command("history")]
public class HistoryCommands : ICommandReceiver
{
    public void Receive(PlayerControl source, CommandContext context)
    {
        if (Game.MatchData.GameHistory == null!) return;
        Utils.SendMessage(Game.MatchData.GameHistory.Events.Where(e => e.IsCompletion()).Select(e => e.GenerateMessage()).Join(delimiter: "\n"), source.PlayerId);
    }
}