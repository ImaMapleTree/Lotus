using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.Managers.History.Events;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Interactions.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Impostors;

[Localized("Roles.Blackmailer")]
public class Blackmailer: Shapeshifter
{
    [Localized("BlackmailMessage")]
    private static string _blackmailedMessage = "You have been blackmailed! Sending a chat message will kill you.";
    [Localized("BlackmailWarning")]
    private static string _warningMessage = "You are not allowed to speak! If you speak again you may be killed.";
    private Remote<TextComponent>? blackmailingText;
    private Optional<PlayerControl> blackmailedPlayer = Optional<PlayerControl>.Null();

    private bool showBlackmailedToAll;

    private int warnsUntilKick;
    private int currentWarnings;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(RoleActionType.Shapeshift)]
    public void Blackmail(PlayerControl target, ActionHandle handle)
    {
        if (target.PlayerId == MyPlayer.PlayerId) return;
        handle.Cancel();
        blackmailingText?.Delete();
        blackmailedPlayer = Optional<PlayerControl>.NonNull(target);
        TextComponent textComponent = new(new LiveString("BLACKMAILED", Color.red), GameStates.IgnStates, viewers: MyPlayer);
        blackmailingText = target.NameModel().GetComponentHolder<TextHolder>().Add(textComponent);
    }

    [RoleAction(RoleActionType.RoundStart, triggerAfterDeath: true)]
    public void ClearBlackmail()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        currentWarnings = 0;
        blackmailingText?.Delete();
    }

    [RoleAction(RoleActionType.RoundEnd)]
    public void NotifyBlackmailed()
    {
        List<PlayerControl> allPlayers = showBlackmailedToAll
            ? Game.GetAllPlayers().ToList()
            : blackmailedPlayer.Transform(p => new List<PlayerControl> { p, MyPlayer }, () => new List<PlayerControl> { MyPlayer });
        if (!blackmailingText?.IsDeleted() ?? false) blackmailingText?.Get().SetViewerSupplier(() => allPlayers);
        blackmailedPlayer.IfPresent(p =>
        {
            string message = $"{RoleColor.Colorize(MyPlayer.name)} blackmailed {p.GetRoleColor().Colorize(p.name)}.";
            Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, p, message));
            Utils.SendMessage(_blackmailedMessage, p.PlayerId);
        });
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void BlackmailerDies()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        blackmailingText?.Delete();
    }

    [RoleAction(RoleActionType.Chat)]
    public void InterceptChat(PlayerControl speaker, GameState state, bool isAlive)
    {
        if (!isAlive || state is not GameState.InMeeting) return;
        if (!blackmailedPlayer.Exists() || speaker.PlayerId != blackmailedPlayer.Get().PlayerId) return;
        if (currentWarnings++ < warnsUntilKick)
        {
            Utils.SendMessage(_warningMessage, speaker.PlayerId);
            return;
        }

        VentLogger.Trace($"Blackmailer Killing Player: {speaker.name}");
        MyPlayer.InteractWith(speaker, new UnblockedInteraction(new FatalIntent(), this));
    }

    public override void HandleDisconnect() => ClearBlackmail();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Warnings Until Death")
                .AddIntRange(0, 5, 1)
                .BindInt(i => warnsUntilKick = i)
                .Build())
            .SubOption(sub => sub.Name("Show Blackmailed to All")
                .AddOnOffValues()
                .BindBool(b => showBlackmailedToAll = b)
                .Build());

    private class PersonalFatalIntent : IFatalIntent
    {
        public void Action(PlayerControl actor, PlayerControl target)
        {
            RpcV3.Immediate(actor.NetId, RpcCalls.MurderPlayer).Write(target).Send(target.GetClientId());
            target.RpcExileV2();
        }

        public void Halted(PlayerControl actor, PlayerControl target)
        {
        }

        public Optional<IDeathEvent> CauseOfDeath() => Optional<IDeathEvent>.Null();

        public bool IsRanged() => false;
    }
}