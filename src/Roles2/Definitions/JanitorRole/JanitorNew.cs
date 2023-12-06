extern alias JBAnnotations;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles2.Attributes.Options;
using Lotus.Roles2.Attributes.Roles;
using Lotus.Roles2.GUI;
using UnityEngine;
using VentLib.Networking.RPC.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2.Definitions.JanitorRole;

[RoleTriggers]
public class JanitorNew: ImpostorRoleDefinition, RoleGUI
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(JanitorNew));

    [BoolOption]
    [RoleLocalized("Clean On Kill")]
    public bool CleanOnKill;

    [RoleLocalized("Kill Cooldown Multiplier")]
    [FloatRangeOption(1, 3, 0.25f, 2, SuffixType.Multiplier)]
    [OptionHierarchyChild(nameof(CleanOnKill), PredicateType.TrueValue)]
    public float KillCooldownMultiplier;

    [UIComponent(UI.Cooldown)]
    public Cooldown CleanCooldown = null!;

    public override float KillCooldown => CleanOnKill ? Defaults.KillCooldown * KillCooldownMultiplier : Defaults.KillCooldown;

    [ModRPC(RoleRPC.RemoveBody, invocation: MethodInvocation.ExecuteAfter)]
    public static void CleanBody(byte playerId)
    {
        log.Debug("Destroying Bodies", "JanitorClean");
        Object.FindObjectsOfType<DeadBody>().ToArray().Where(db => db.ParentId == playerId).ForEach(b => Object.Destroy(b.gameObject));
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        CleanCooldown.Start(AUSettings.KillCooldown());

        if (!CleanOnKill) return base.TryKill(target);

        MyPlayer.RpcMark(target);
        if (MyPlayer.InteractWith(target, new LotusInteraction(new FakeFatalIntent(), this)) is InteractionResult.Halt) return false;
        MyPlayer.RpcVaporize(target);
        return true;
    }

    [RoleAction(LotusActionType.ReportBody)]
    private void JanitorCleanBody(GameData.PlayerInfo target, ActionHandle handle)
    {
        if (CleanCooldown.NotReady()) return;
        handle.Cancel();
        CleanCooldown.Start();

        byte playerId = target.Object.PlayerId;

        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>())
            if (deadBody.ParentId == playerId)
                if (ModVersion.AllClientsModded()) CleanBody(playerId);
                else Game.MatchData.UnreportableBodies.Add(playerId);

        MyPlayer.RpcMark(MyPlayer);
    }

    public RoleButton ReportButton(RoleButtonEditor reportButton) => reportButton.LoadSpriteAndTextOnButton("Clean", "Lotus.assets.roles.janitor.janitor_clean.png", 950);
}