using System.Linq;
using TOHTOR.API.Stats;
using TOHTOR.Extensions;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Chat.Commands;

[Localized("Commands.Stats")]
[Command(CommandFlag.LobbyOnly, "stats", "stat")]
public class StatCommand: ICommandReceiver
{
    [Localized("PlayerNotFound")]
    private static string _playerNotFoundMessage = "Player \"{0}\" not found";

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) GetPlayerStats(source, source);
        else GetPlayerStats(source, context.Join());
    }

    private void GetPlayerStats(PlayerControl requester, string name)
    {
        Optional<PlayerControl> searchedPlayer = PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(p => p.Data.GetPlayerName(PlayerOutfitType.Default) == name);
        searchedPlayer.Handle(
            player => GetPlayerStats(requester, player),
            () => Utils.SendMessage(string.Format(_playerNotFoundMessage, name), requester.PlayerId)
        );
    }

    private void GetPlayerStats(PlayerControl requester, PlayerControl target)
    {
        string statisticMessage = $"Statistics for Player: {target.name}\n";

        statisticMessage = Statistics.Current().GetAllStats()
            .Aggregate(statisticMessage, (current, statistic) =>
            {
                object? value = statistic is IAccumulativeStatistic accumulative
                    ? accumulative.GenericAccumulativeValue(target.UniquePlayerId())
                    : statistic.GetGenericValue(target.UniquePlayerId());

                return current + $"{statistic.Name()}: {value}\n";
            });

        Utils.SendMessage(statisticMessage, requester.PlayerId, "Statistics");
    }
}