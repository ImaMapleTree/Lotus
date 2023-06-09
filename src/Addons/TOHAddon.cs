using System;
using System.Collections.Generic;
using System.Reflection;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Extensions;

namespace Lotus.Addons;

public abstract class TOHAddon
{
    internal readonly List<CustomRole> CustomRoles = new();
    internal readonly List<IFaction> Factions = new();
    internal readonly List<Type> Gamemodes = new();

    internal readonly Assembly BundledAssembly = Assembly.GetCallingAssembly();
    internal readonly ulong Uuid;

    public TOHAddon()
    {
        Uuid = (BundledAssembly.GetIdentity(false)?.SemiConsistentHash() ?? 0ul + AddonName().SemiConsistentHash());
    }

    internal string GetName(bool fullName = false) => !fullName
        ? AddonName()
        : $"{BundledAssembly.FullName}::{AddonName()}-{AddonVersion()}";

    public abstract void Initialize();

    public abstract string AddonName();

    public abstract string AddonVersion();

    public void RegisterRole(CustomRole customRole) => CustomRoles.Add(customRole);

    public void RegisterGamemode(Type gamemode) => Gamemodes.Add(gamemode);

    public void RegisterFaction(IFaction factionOld) => Factions.Add(factionOld);

    public override string ToString() => GetName(true);
}

