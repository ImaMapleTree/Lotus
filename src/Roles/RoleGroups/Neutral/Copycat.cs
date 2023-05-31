using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers;
using Lotus.Managers.History.Events;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.Logging;
using Lotus.Roles.Interfaces;
using Lotus.Roles.RoleGroups.Impostors;
using Lotus.RPC;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using Random = UnityEngine.Random;

namespace Lotus.Roles.RoleGroups.Neutral;

public class Copycat: CustomRole, IVariableRole
{
    public static SchrodingersCat? SchrodingersCat;
    /// <summary>
    /// A dict of role types and roles for the cat to fallback upon if the role cannot be copied properly (ex: Crewpostor bc Copycat cannot gain tasks)
    /// </summary>
    public static readonly Dictionary<Type, Func<CustomRole>> FallbackTypes = new()
    {
        {typeof(CrewPostor), () => CustomRoleManager.Static.Madmate },
        {typeof(Mafioso), () => CustomRoleManager.Static.Impostor }
    };

    public bool KillerKnowsCopycat;
    private bool copyIdentity;
    private bool copyRoleProgress;
    private bool turned;

    public override bool CanVent() => false;

    public CustomRole Variation() => SchrodingersCat!;

    public bool AssignVariation() => Random.RandomRange(0, 100) <= SchrodingersCat!.Chance;

    [RoleAction(RoleActionType.Interaction)]
    protected void CopycatAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (turned || interaction.Intent() is not IFatalIntent) return;
        turned = true;
        AssignRole(actor);
        handle.Cancel();
    }

    [RoleAction(RoleActionType.Shapeshift)]
    private void PreventShapeshift(ActionHandle handle) => handle.Cancel();

    private void AssignRole(PlayerControl attacker)
    {
        CustomRole attackerRole = attacker.GetCustomRole();
        FallbackTypes.GetOptional(attackerRole.GetType()).IfPresent(r => attackerRole = r());
        CustomRole role = copyRoleProgress ? attackerRole : CustomRoleManager.GetCleanRole(attackerRole);

        VentLogger.Trace($"Copycat ({MyPlayer.name}) copying role of {attacker.name} : {role.RoleName}", "Copycat::AssignRole");
        MatchData.AssignRole(MyPlayer, role);

        role = MyPlayer.GetCustomRole();
        role.RoleColor = RoleColor;

        role.OverridenRoleName = Translations.CatFactionChangeName.Formatted(role.RoleName);
        RoleComponent roleComponent = MyPlayer.NameModel().GCH<RoleHolder>().Last();
        roleComponent.SetMainText(new LiveString(role.RoleName, RoleColor));
        Players.GetAllPlayers().Where(p => role.Relationship(p) is Relation.FullAllies).ForEach(p => roleComponent.AddViewer(p));

        if (attacker.Relationship(MyPlayer) is Relation.FullAllies) attacker.NameModel().GCH<RoleHolder>().First().AddViewer(MyPlayer);

        Game.MatchData.GameHistory.AddEvent(new RoleChangeEvent(MyPlayer, role, this));

        float killCooldown = role.GetOverride(Override.KillCooldown)?.GetValue() as float? ?? AUSettings.KillCooldown();
        role.SyncOptions(new[] { new GameOptionOverride(Override.KillCooldown, killCooldown * 2) });
        Async.Schedule(() =>
        {
            MyPlayer.RpcMark(MyPlayer);
            role.SyncOptions();
        }, NetUtils.DeriveDelay(0.05f));


        if (!role.GetActions(RoleActionType.Shapeshift).Any())
        {
            VentLogger.Trace("Adding shapeshift action to base role", "Copycat::AssignRole");
            RoleAction action = this.GetActions(RoleActionType.Shapeshift).First().Item1.Clone();
            action.Executer = this;

            role.Editor = new BasicRoleEditor(role);
            role.Editor!.AddAction(action);
        }

        if (copyIdentity) Async.Schedule(() => MyPlayer.CRpcShapeshift(attacker, false), 2);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub2 => sub2.KeyName("Copy Role's Progress", Translations.Options.CopyRoleProgress)
                    .AddOnOffValues(false)
                    .BindBool(b => copyRoleProgress = b)
                    .Build())
            .SubOption(sub2 => sub2.KeyName("Shapeshift Into Attacker", Translations.Options.ShapeshiftIntoAttacker)
                .AddOnOffValues(false)
                .BindBool(b => copyIdentity = b)
                .Build())
            .SubOption(sub => sub.KeyName("Killer Knows Copycat", TranslationUtil.Colorize(Translations.Options.KillerKnowsCopycat, RoleColor))
                .AddOnOffValues()
                .BindBool(b => KillerKnowsCopycat = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.RoleColor(new Color(1f, 0.7f, 0.67f))
            .VanillaRole(RoleTypes.Shapeshifter)
            .Faction(FactionInstances.Solo)
            .RoleFlags(RoleFlag.CannotWinAlone)
            .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage)
            .SpecialType(SpecialType.Neutral)
            .LinkedRoles(SchrodingersCat ??= new SchrodingersCat());

    [Localized(nameof(Copycat))]
    private static class Translations
    {
        [Localized(nameof(CatFactionChangeName))]
        public static string CatFactionChangeName = "{0}cat";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string CopyAttackersRole = "Copy Attacker's Role";
            public static string CopyRoleProgress = "Copy Role's Progress";
            public static string ShapeshiftIntoAttacker = "Shapeshift Into Attacker";
            public static string KillerKnowsCopycat = "Killer Knows Copycat::0";
        }
    }
}