using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

public class StatusCommand: CommandTranslations
{
    [Command("status")]
    public static void StatusEntry(PlayerControl source, CommandContext context)
    {
        if (!Game.MatchData.FrozenPlayers.Any()) ErrorHandler(source).Message(NoPreviousGameText).Send();
        else if (context.Args.Length == 0) DisplayPlayerStatus(source, Game.MatchData.FrozenPlayers[source.GetGameID()]);
        else
        {
            if (int.TryParse(context.Join(), out int id)) DisplayStatusFromID(source, id);
            else DisplayStatusFromName(source, context.Join());
        }
    }

    public static string GetPlayerStatus(FrozenPlayer player)
    {
        return player.Statuses.Select(s => $"{s.Color.Colorize(s.Name)}\n{s.Description}").Fuse("\n\n");
    }

    private static void DisplayPlayerStatus(PlayerControl source, FrozenPlayer frozenPlayer)
    {
        CHandler(GetPlayerStatus(frozenPlayer)).Send(source);
    }

    private static void DisplayStatusFromName(PlayerControl source, string name)
    {
        Game.MatchData.FrozenPlayers.Values.FirstOrOptional(fp => fp.Name.ToLower().Contains(name.ToLower()))
            .Handle(fp => DisplayPlayerStatus(source, fp), () => CHandler(PlayerNotFoundText.Formatted(name)).Send(source));
    }

    private static void DisplayStatusFromID(PlayerControl source, int id)
    {
        Game.MatchData.FrozenPlayers.Values.FirstOrOptional(fp => fp.PlayerId == id)
            .Handle(fp => DisplayPlayerStatus(source, fp), () => CHandler(PlayerNotFoundText.Formatted(id)).Send(source));
    }

    private static ChatHandler ErrorHandler(PlayerControl source) => new ChatHandler().Title(t => t.Text(CommandError).Color(ModConstants.Palette.KillingColor).Build()).Player(source).LeftAlign();

    private static ChatHandler CHandler(string message)
    {
        return ChatHandler.Of(message, new Color(0.71f, 1f, 0.44f).Colorize(Translations.StatusTitle)).LeftAlign();
    }

    [Localized("Status")]
    private static class Translations
    {
        public static string StatusTitle = "Player Status";
    }
}