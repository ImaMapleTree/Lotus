using System.Collections.Generic;
using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.Factions.Interfaces;
using TOHTOR.Factions.Undead;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.GUI.Name.Interfaces;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Undead.Events;
using TOHTOR.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Undead.Roles;

public class UndeadRole : Impostor
{
    public static Color UndeadColor = new(0.33f, 0.46f, 0.76f);

    public override bool CanSabotage() => false;

    public void ConvertToUndead(PlayerControl target)
    {
        List<PlayerControl> viewers = Game.GetAlivePlayers().Where(IsConvertedUndead).ToList();

        INameModel nameModel = target.NameModel();

        string unalteredName = nameModel.Unaltered();
        string fullName = UndeadColor.Colorize(unalteredName[..(unalteredName.Length / 2)] + Color.white.Colorize(unalteredName[(unalteredName.Length / 2)..]));
        NameComponent nameComponent = new(new LiveString(fullName), new[] { GameState.Roaming, GameState.InMeeting }, ViewMode.Replace, () => viewers);

        nameModel.GetComponentHolder<NameHolder>().Add(nameComponent);
        viewers.ForEach(v => nameModel.GetComponentHolder<RoleHolder>()[0].AddViewer(v));

        CustomRole role = target.GetCustomRole();
        role.Faction = new TheUndead.Unconverted(role.Faction, nameComponent);
        role.SpecialType = SpecialType.Undead;
        Game.GameHistory.AddEvent(new ConvertEvent(MyPlayer, target));
    }

    public void InitiateUndead(PlayerControl target)
    {
        List<PlayerControl> undead = Game.GetAlivePlayers().Where(IsConvertedUndead).ToList();
        List<PlayerControl> viewers = new() { target };

        LiveString undeadPlayerName = new(target.NameModel().Unaltered(), UndeadColor);

        if (target.GetCustomRole().Faction is TheUndead.Unconverted unconverted)
        {
            NameComponent oldComponent = unconverted.UnconvertedName;
            oldComponent.SetMainText(undeadPlayerName);
            oldComponent.AddViewer(target);
        } else {
            NameComponent newComponent = new(undeadPlayerName, new[] { GameState.Roaming, GameState.InMeeting }, ViewMode.Replace, () => viewers);
            target.NameModel().GetComponentHolder<NameHolder>().Add(newComponent);
        }

        target.GetCustomRole().Faction = FactionInstances.TheUndead;

        undead.ForEach(p =>
        {
            INameModel nameModel = p.NameModel();
            nameModel.GetComponentHolder<RoleHolder>()[0].AddViewer(target);

            switch (p.GetCustomRole().Faction)
            {
                case TheUndead.Converted converted:
                    converted.NameComponent.AddViewer(target);
                    break;
                case TheUndead.Origin:
                    nameModel.GetComponentHolder<NameHolder>()[0].AddViewer(target);
                    break;
                default:
                    nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(new LiveString(nameModel.Unaltered, UndeadColor), new [] { GameState.Roaming, GameState.InMeeting}, ViewMode.Replace, viewers: () => viewers));
                    break;
            }
        });

        Game.GameHistory.AddEvent(new InitiateEvent(MyPlayer, target));
    }

    protected static bool IsUnconvertedUndead(PlayerControl player) => player.GetCustomRole().Faction is TheUndead.Unconverted;
    protected static bool IsConvertedUndead(PlayerControl player)
    {
        IFaction faction = player.GetCustomRole().Faction;
        if (faction is not TheUndead) return false;
        return faction is not TheUndead.Unconverted;
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .SpecialType(SpecialType.Undead)
            .Faction(FactionInstances.TheUndead);
}