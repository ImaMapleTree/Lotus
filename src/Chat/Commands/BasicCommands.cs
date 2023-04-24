using System;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Chat.Commands;

[Command("r", "rename")]
public class Rename: ICommandReceiver
{
    public void Receive(PlayerControl source, CommandContext context)
    {
        //if (!(StaticOptions.AllowCustomizeCommands || source.IsHost())) return;
        string name = String.Join(" ", context.Args);
        source.RpcSetName(name);
    }
}

[Command("w", "winner")]
public class Winner : ICommandReceiver
{
    public void Receive(PlayerControl source, CommandContext _)
    {
        Utils.SendMessage($"Winners: {String.Join(", ", Game.GameHistory.LastWinners)}", source.PlayerId);
    }
}

[Localized("Commands.Admin")]
[Command(new []{ "kick", "ban" }, user: CommandUser.Host)]
public class KickCommand : ICommandReceiver
{
    [Localized("KickMessage")] private static string _kickedMessage = "{0} was kicked by host.";
    [Localized("BanMessage")] private static string _banMessage = "{0} was banned by host.";

    public void Receive(PlayerControl source, CommandContext context)
    {
        bool ban = context.Alias == "ban";
        string message = ban ? _banMessage : _kickedMessage;

        if (context.Args.Length == 0)
        {
            Utils.SendMessage("Invalid Usage. Requires either a number or name.", source.PlayerId, "Announcement");
            return;
        }

        Optional<PlayerControl> targetPlayer = Optional<PlayerControl>.Null();
        string text = context.Join();
        if (int.TryParse(text, out int result)) targetPlayer = Utils.PlayerById(result);
        else targetPlayer = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrOptional(p => p.UnalteredName().ToLowerInvariant().Equals(text.ToLowerInvariant()));

        targetPlayer.Handle(player =>
        {
            AmongUsClient.Instance.KickPlayer(player.GetClientId(), ban);
            Utils.SendMessage(string.Format(message, player.UnalteredName()), title: "Announcement");
        }, () => Utils.SendMessage($"Unable to find player: {text}", source.PlayerId, "Announcement"));
    }
}