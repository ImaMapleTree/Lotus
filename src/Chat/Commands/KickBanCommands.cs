using Lotus.Extensions;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Roles.Interactions;
using Lotus.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Chat.Commands;

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
        bool banWithId = false;
        if (int.TryParse(context.Args[0], out int result))
        {
            targetPlayer = Utils.PlayerById(result);
            banWithId = true;
        }
        else targetPlayer = PlayerControl.AllPlayerControls.ToArray()
                .FirstOrOptional(p => p.name.ToLowerInvariant().Equals(text.ToLowerInvariant()));

        targetPlayer.Handle(player =>
        {
            if (LobbyBehaviour.Instance == null) player.RpcExileV2(false);
            if (banWithId && context.Args.Length > 1)
            {
                string reason = context.Args[1..].Fuse(" ");
                PluginDataManager.BanManager.BanWithReason(player, reason);
            }
            else AmongUsClient.Instance.KickPlayer(player.GetClientId(), ban);
            ChatHandler.Of(message.Formatted(player.name), "Announcement").Send();
        }, () => ChatHandler.Of($"Unable to find player: {text}", "Announcement").Send(source));
    }
}