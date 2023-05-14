using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
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
using VentLib.Utilities.Optionals;

namespace Lotus.Roles.RoleGroups.Crew;

[Localized($"Roles.{nameof(Oracle)}")]
public class Oracle: Crewmate
{
    private static ColorGradient _oracleGradient = new(new Color(0.49f, 0.57f, 0.84f), new Color(0.67f, 0.36f, 0.76f));
    
    private Optional<byte> selectedPlayer = Optional<byte>.Null();
    private bool targetLockedIn;
    private bool initialSkip;

    [Localized("VotePlayerInfo")]
    private static string votePlayerMessage = "Vote to select a player to reveal on your death. You can re-vote a player to unselect them.\nAfter confirming your target cannot be changed.";
    [Localized("SelectRole")]
    private static string selectRoleMsg = "You have selected:";
    [Localized("UnselectRole")]
    private static string unselectRoleMsg = "You have unselected:";
    private static string skipMsg = "Press \"Skip Vote\" to continue.";

    [RoleAction(RoleActionType.RoundEnd)]
    private void OracleSendMessage()
    {
        initialSkip = false;
        if (selectedPlayer.Exists()) return;
        Utils.SendMessage(votePlayerMessage, MyPlayer.PlayerId);
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
                _ => Utils.SendMessage($"{selectRoleMsg} {target.Get().name}\n{skipMsg}", MyPlayer.PlayerId, "Oracle Ability"),
                () =>
                {
                    Utils.SendMessage("You may now vote normally", MyPlayer.PlayerId, "Oracle Ability");
                    initialSkip = true;
                }
            );
            return;
        }

        if (!target.Exists())
        {
            Utils.SendMessage("You may now vote normally", MyPlayer.PlayerId, "Oracle Ability");
            targetLockedIn = true;
            return;
        }

        if (selectedPlayer.Get() == target.Get().PlayerId)
        {
            selectedPlayer = Optional<byte>.Null();
            Utils.SendMessage($"{unselectRoleMsg} {target.Get().name}\n{skipMsg}", MyPlayer.PlayerId, "Oracle Ability");
            return;
        }

        selectedPlayer = target.Map(p => p.PlayerId);
        Utils.SendMessage($"{selectRoleMsg} {target.Get().name}\n{skipMsg}", MyPlayer.PlayerId, "Oracle Ability");
    }

    [RoleAction(RoleActionType.MyDeath)]
    private void OracleDies()
    {
        if (!selectedPlayer.Exists()) return;
        PlayerControl target = Utils.GetPlayerById(selectedPlayer.Get())!;
        target.NameModel().GetComponentHolder<RoleHolder>().Last(c => c.ViewMode() is ViewMode.Replace).SetViewerSupplier(() => Game.GetAllPlayers().ToList());

        string roleName = _oracleGradient.Apply(target.GetCustomRole().RoleName);

        target.NameModel().GetComponentHolder<RoleHolder>().Add(new RoleComponent(new LiveString(() => roleName), GameStates.IgnStates, ViewMode.Replace));
    }

    [RoleAction(RoleActionType.Disconnect)]
    private void TargetDisconnected(PlayerControl dcPlayer)
    {
        if (!selectedPlayer.Exists() || selectedPlayer.Get() != dcPlayer.PlayerId) return;
        selectedPlayer = Optional<byte>.Null();
        targetLockedIn = false;
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.78f, 0.55f, 0.82f));
}