using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Optionals;

namespace TOHTOR.Roles.RoleGroups.Crew;

[Localized($"Roles.{nameof(Oracle)}")]
public class Oracle: Crewmate
{
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
    }

    [RoleAction(RoleActionType.OnDisconnect)]
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