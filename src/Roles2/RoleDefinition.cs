using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using Lotus.Addons;
using Lotus.API;
using Lotus.API.Stats;
using Lotus.Extensions;
using Lotus.Factions.Interfaces;
using Lotus.Roles.Overrides;
using Lotus.Roles2.Attributes;
using Lotus.Roles2.GUI;
using Lotus.Roles2.Interfaces;
using UnityEngine;
using VentLib.Options.Game.Interfaces;
using VentLib.Utilities.Collections;

namespace Lotus.Roles2;

public abstract class RoleDefinition: IUnifiedDefinitionAware, IPostLinkExecuter
{
    public static NamespacedKey<string> DescriptionKey = NamespacedKey.Lotus<string>(nameof(DescriptionKey));
    public static NamespacedKey<string> BlurbKey = NamespacedKey.Lotus<string>(nameof(BlurbKey));
    public static NamespacedKey<int> DisplayOrder = NamespacedKey.Lotus<int>(nameof(DisplayOrder));

    public static NamespacedKey<List<Statistic>> Statistics = NamespacedKey.Lotus<List<Statistic>>(nameof(Statistics));

    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(RoleDefinition));

    [SetupInjected.Excluded]
    private OrderedSet<GameOptionOverride> overrides = new();

    public string TypeName => GetType().Name;
    public virtual string Name => Localizer.ProvideTranslation($"Roles.{TypeName}.Name", TypeName);
    public abstract RoleTypes Role { get; }
    public abstract Color RoleColor { get; set; }
    public abstract IFaction Faction { get; set; }
    public abstract IGameOptionTab OptionTab { get; }

    /// <summary>
    /// Represents an ID for the role that should be unique to THE defining assembly
    /// </summary>
    public virtual ulong RoleID => this.GetType().SemiConsistentHash();

    internal virtual bool CanVentInternal => true;
    internal virtual bool CanSabotageInternal => true;
    internal Assembly Assembly => this.GetType().Assembly;
    internal LotusAddon? Addon;

    public virtual RoleLocalizer Localizer { get; } = new DefaultRoleLocalizer();
    public virtual GUIProvider GUIProvider { get; } = new();
    protected internal virtual RoleMetadata Metadata { get; } = new();

    [SetupInjected.Excluded]
    public PlayerControl MyPlayer { get; private set; } = null!;

    [SetupInjected.Excluded]
    public UnifiedRoleDefinition Handle { get; private set; } = null!;

    public void AddGameOptionOverride(Override overrideType, Func<object> supplier, Func<bool>? condition = null)
    {
        (Handle?.UnifiedRoleOverrides ?? overrides).Add(new GameOptionOverride(overrideType, supplier, condition));
    }

    public void AddGameOptionOverride(Override overrideType, object? value, Func<bool>? condition = null)
    {
        (Handle?.UnifiedRoleOverrides ?? overrides).Add(new GameOptionOverride(overrideType, value, condition));
    }

    public void AddGameOptionOverride(GameOptionOverride optionOverride)
    {
        (Handle?.UnifiedRoleOverrides ?? overrides).Add(optionOverride);
    }

    public IList<GameOptionOverride> GetOverrides() => overrides;

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player)
    {
        RoleDefinition copiedDefinition = setupHelper.Clone(this);
        copiedDefinition.MyPlayer = player;
        return copiedDefinition;
    }

    public void PostLinking()
    {
        Metadata.Set(DescriptionKey, Localizer.ProvideTranslation($"Roles.{GetType().Name}.Description", null));
        Metadata.Set(BlurbKey, Localizer.ProvideTranslation($"Roles.{GetType().Name}.Blurb", null));
    }

    public void SetUnifiedDefinition(UnifiedRoleDefinition unifiedRoleDefinition)
    {
        Handle = unifiedRoleDefinition;
    }
}