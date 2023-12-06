using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Options;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game.Interfaces;
using VentLib.Utilities.Collections;

namespace Lotus.Roles2.Definitions;

public class NoOpDefinition: UnifiedRoleDefinition
{
    public static NoOpDefinition Instance = new();

    internal NoOpDefinition() : base(new NoOpRoleDefinition(), new ComponentInstanceManager(new OrderedSet<Type>()), new Dictionary<LotusActionType, List<RoleActionStub>>())
    {
    }

    private class NoOpRoleDefinition : RoleDefinition
    {
        public override string Name => "NO-OP";
        public override Color RoleColor { get; set; } = Color.gray;
        public override RoleTypes Role => RoleTypes.Crewmate;
        public override IFaction Faction { get; set; } = FactionInstances.Crewmates;
        public override IGameOptionTab OptionTab => DefaultTabs.HiddenTab;
        public override ulong RoleID => 999999;
    }
}