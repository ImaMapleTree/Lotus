using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Chat;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.GUI.Name.Impl;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

public class Oracle: Crewmate
{
    private static ColorGradient _oracleGradient = new(new Color(0.49f, 0.57f, 0.84f), new Color(0.67f, 0.36f, 0.76f));
    
    private Optional<byte> selectedPlayer = Optional<byte>.Null();
    private bool targetLockedIn;
    private bool initialSkip;
    

    [RoleAction(RoleActionType.RoundEnd)]
    private void OracleSendMessage()
    {
        initialSkip = false;
        if (selectedPlayer.Exists()) return;
        CHandler().Message(Translations.VotePlayerInfo).Send(MyPlayer);
    }

    [RoleAction(RoleActionType.MyVote)]
    private void OracleLockInTarget(Optional<PlayerControl> target, ActionHandle handle)
    {
        if (targetLockedIn || initialSkip) return;
        handle.Cancel();

        if (!selectedPlayer.Exists())
        {
            selectedPlayer = target.Map(p => p.PlayerId);
            selectedPlayer.Handle(
                _ => CHandler().Message($"{Translations.SelectRole.Formatted(target.Get().name)}\n{Translations.SkipMessage}").Send(MyPlayer),
                () =>
                {
                    CHandler().Message(Translations.VoteNormallyMessage).Send(MyPlayer);
                    initialSkip = true;
                }
            );
            return;
        }

        if (!target.Exists())
        {
            CHandler().Message(Translations.VoteNormallyMessage).Send(MyPlayer);
            targetLockedIn = true;
            return;
        }

        if (selectedPlayer.Get() == target.Get().PlayerId)
        {
            selectedPlayer = Optional<byte>.Null();
            CHandler().Message($"{Translations.UnselectRole.Formatted(target.Get().name)}\n{Translations.SkipMessage}").Send(MyPlayer);
            return;
        }

        selectedPlayer = target.Map(p => p.PlayerId);
        CHandler().Message($"{Translations.SelectRole.Formatted(target.Get().name)}\n{Translations.SkipMessage}").Send(MyPlayer);
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void OracleDies()
    {
        if (!selectedPlayer.Exists()) return;
        PlayerControl target = Utils.GetPlayerById(selectedPlayer.Get())!;
        target.NameModel().GetComponentHolder<RoleHolder>().Last(c => c.ViewMode() is ViewMode.Replace).SetViewerSupplier(() => Game.GetAllPlayers().ToList());

        string roleName = _oracleGradient.Apply(target.GetCustomRole().RoleName);

        target.NameModel().GetComponentHolder<RoleHolder>().Add(new RoleComponent(new LiveString(() => roleName), GameStates.IgnStates, ViewMode.Replace));
        CHandler().Message(Translations.RevealMessage, target.name, roleName).Send();
    }

    [RoleAction(RoleActionType.Disconnect)]
    private void TargetDisconnected(PlayerControl dcPlayer)
    {
        if (!selectedPlayer.Exists() || selectedPlayer.Get() != dcPlayer.PlayerId) return;
        selectedPlayer = Optional<byte>.Null();
        targetLockedIn = false;
    }

    private ChatHandler CHandler() => ChatHandler.Of(title: _oracleGradient.Apply(Translations.OracleMessageTitle)).LeftAlign();

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.78f, 0.55f, 0.82f));

    [Localized(nameof(Oracle))]
    private static class Translations
    {
        [Localized(nameof(OracleMessageTitle))]
        public static string OracleMessageTitle = "Oracle Ability";
        
        [Localized(nameof(VotePlayerInfo))]
        public static string VotePlayerInfo = "Vote to select a player to reveal on your death. You can re-vote a player to unselect them.\nAfter confirming your target cannot be changed.";
        
        [Localized(nameof(SelectRole), ForceOverride = true)]
        public static string SelectRole = "You have selected: {0}";
        
        [Localized(nameof(UnselectRole), ForceOverride = true)]
        public static string UnselectRole = "You have unselected: {0}";

        [Localized(nameof(VoteNormallyMessage))]
        public static string VoteNormallyMessage = "You may now vote normally";
        
        [Localized(nameof(SkipMessage))]
        public static string SkipMessage = "Press \"Skip Vote\" to continue.";

        [Localized(nameof(RevealMessage))]
        public static string RevealMessage = "The Oracle has revealed to all that {0} is the {1}::0";
    }
}