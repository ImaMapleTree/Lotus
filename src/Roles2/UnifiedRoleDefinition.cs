using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazel;
using Lotus.Addons;
using Lotus.API;
using Lotus.API.Stats;
using Lotus.Factions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Roles2.Components;
using Lotus.Roles2.Components.LSI;
using Lotus.Roles2.GUI;
using Lotus.Roles2.Manager;
using Lotus.Roles2.Operations;
using UnityEngine;
using VentLib.Networking;
using VentLib.Networking.Interfaces;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles2;

public class UnifiedRoleDefinition: IRpcSendable<UnifiedRoleDefinition>
{
    static UnifiedRoleDefinition() => AbstractConstructors.Register(typeof(UnifiedRoleDefinition), r => IRoleManager.Current.GetRole(r.ReadString()));

    private static readonly List<RoleAction> NoActions = new();

    public string GlobalRoleID => $"${(Metadata.GetOrDefault(LotusKeys.GloballyManagedRole, false) ? "G" : "")}{Addon?.UUID ?? 0}~{RoleID}";

    public OrderedSet<GameOptionOverride> UnifiedRoleOverrides { get; private set; } = new();
    public string Name => RoleDefinition.Name;
    public string TypeName => RoleDefinition.TypeName;
    public string Description => Metadata.GetOrDefault(RoleDefinition.DescriptionKey, "");
    public string Blurb => Metadata.GetOrDefault(RoleDefinition.BlurbKey, "");
    public List<Statistic> Statistics => Metadata.GetOrEmpty(RoleDefinition.Statistics).OrElseGet(() => new List<Statistic>());

    internal int DisplayOrder => Metadata.GetOrDefault(RoleDefinition.DisplayOrder, 0);


    public Color RoleColor { get => RoleDefinition.RoleColor; set => RoleDefinition.RoleColor = value; }
    public IFaction Faction { get => RoleDefinition.Faction; set => RoleDefinition.Faction = value; }

    public int Chance;
    public int Count;
    public int AdditionalChance;

    public RoleMetadata Metadata { get; private set; }
    public PlayerControl MyPlayer => RoleDefinition.MyPlayer;

    internal Assembly Assembly => RoleDefinition.Assembly;
    internal ulong RoleID => RoleDefinition.RoleID;
    internal LotusAddon? Addon => RoleDefinition.Addon;

    internal RoleDefinition RoleDefinition;
    internal Dictionary<LotusActionType, List<RoleAction>> Actions = new();
    internal OptionConsolidator OptionConsolidator;
    internal RelationshipConsolidator RelationshipConsolidator;
    internal RoleOperations? RoleOperations { get; }

    private Dictionary<LotusActionType, List<RoleActionStub>> actionStubs;
    private GeneratingCIM generatingCIM;

    private Func<bool> canSabotageDelegate = () => true;
    private Func<bool> canVentDelegate = () => true;

    public UnifiedRoleDefinition(RoleDefinition roleDefinition, ComponentInstanceManager componentInstanceManager, Dictionary<LotusActionType, List<RoleActionStub>> actionStubs)
    {
        this.RoleDefinition = roleDefinition;
        this.generatingCIM = componentInstanceManager.Generate(this);
        this.actionStubs = actionStubs;
        this.generatingCIM.GenerateInstances();
        this.OptionConsolidator = this.generatingCIM.FindComponent<OptionConsolidator>()!;
        this.RelationshipConsolidator = this.generatingCIM.FindComponent<RelationshipConsolidator>()!;
        this.Metadata = this.RoleDefinition.Metadata.Combine(this.generatingCIM.FindComponents<RoleMetadata>());
    }

    public bool IsEnabled() => this.Chance > 0 && this.Count > 0;

    public bool CanSabotage() => canSabotageDelegate();

    public bool CanVent() => canVentDelegate();

    public RoleOperations GetRoleOperations() => RoleOperations ?? IRoleManager.Current.RoleOperations;

    public List<RoleAction> GetActions(LotusActionType actionType) => Actions.GetValueOrDefault(actionType, NoActions);

    public GameOptionOverride GetOverride(Override overrideType) => UnifiedRoleOverrides.FirstOrDefault(o => o.Option == overrideType)!;

    public void SyncOptions() => GetRoleOperations().SyncOptions(this.MyPlayer, this);

    public GeneratingCIM GetGeneratingCIM()
    {
        return generatingCIM;
    }

    public UnifiedRoleDefinition Instantiate(PlayerControl player)
    {
        UnifiedRoleDefinition copiedDefinition = (UnifiedRoleDefinition)this.MemberwiseClone();

        SetupHelper setupHelper = new();
        copiedDefinition.RoleDefinition = (RoleDefinition) copiedDefinition.RoleDefinition.Instantiate(setupHelper, player);
        copiedDefinition.UnifiedRoleOverrides = new OrderedSet<GameOptionOverride>(UnifiedRoleOverrides);
        GeneratingCIM currentCIM = copiedDefinition.generatingCIM = generatingCIM.CloneAndInstantiate(setupHelper, copiedDefinition);
        copiedDefinition.Actions = currentCIM.DefineActions(actionStubs, copiedDefinition.Actions);

        // Move logic to role manager
        copiedDefinition.Metadata = copiedDefinition.RoleDefinition.Metadata.Combine(currentCIM.FindComponents<RoleMetadata>());

        return copiedDefinition;
    }

    public UnifiedRoleDefinition Read(MessageReader reader)
    {
        string globalRoleId = reader.ReadString();
        try {
            return IRoleManager.Current.GetRole(globalRoleId);
        }
        catch
        {
            foreach (IRoleManager roleManager in ProjectLotus.GameModeManager.GameModes.Select(r => r.RoleManager))
            {
                try {
                    return roleManager.GetRole(globalRoleId);
                }
                catch {
                    /* ignored */
                }
            }

            return IRoleManager.Current.DefaultDefinition;
        }
    }

    public void Write(MessageWriter writer)
    {
        writer.Write(GlobalRoleID);
    }
}