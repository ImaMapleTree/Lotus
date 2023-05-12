using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Chat.Commands;

[Localized("Commands.Admin")]
[Command(CommandFlag.HostOnly, "kick", "ban")]
public class KickBanCommand : ICommandReceiver
{
    [Localized("KickMessage")] private static string _kickedMessage = "{0} was kicked by host.";
    [Localized("BanMessage")] private static string _banMessage = "{0} was banned by host.";

    public void Receive(PlayerControl source, CommandContext context)
    {
        bool ban = context.Alias == "ban";
        string message = ban ? _banMessage : _kickedMessage;

        if (context.Args.Length == 0)
        {
            BasicCommands.PlayerIds(source, context);
            return;
        }

        Optional<PlayerControl> targetPlayer = Optional<PlayerControl>.Null();
        string text = context.Join();
        if (int.TryParse(text, out int result)) targetPlayer = Utils.PlayerById(result);
        else targetPlayer = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrOptional(p => p.name.ToLowerInvariant().Equals(text.ToLowerInvariant()));

        targetPlayer.Handle(player =>
        {
            AmongUsClient.Instance.KickPlayer(player.GetClientId(), ban);
            Utils.SendMessage(string.Format(message, player.name), title: "Announcement");
        }, () => Utils.SendMessage($"Unable to find player: {text}", source.PlayerId, "Announcement"));
    }
}