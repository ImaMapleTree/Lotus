using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.Factions;
using TOHTOR.GUI;
using TOHTOR.GUI.Counters;
using TOHTOR.GUI.Name;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Impl;
using TOHTOR.GUI.Name.Interfaces;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Roles.Subroles;
using VentLib.Logging;
using VentLib.Networking.Interfaces;
using VentLib.Networking.Managers;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles;

public abstract class CustomRole : AbstractBaseRole, IRpcSendable<CustomRole>
{
    static CustomRole()
    {
        AbstractConstructors.Register(typeof(CustomRole), r => CustomRoleManager.GetRoleFromId(r.ReadInt32()));
    }


    public virtual bool CanVent() => BaseCanVent || StaticOptions.AllRolesCanVent;
    public virtual bool CanBeKilled() => !Invincible;
    public virtual bool HasTasks() => this is Crewmate;
    public bool IsDesyncRole() => this.DesyncRole != null;
    public virtual Relation Relationship(PlayerControl player) => this.Relationship(player.GetCustomRole());

    public bool Invincible;

    private HashSet<GameOptionOverride> currentOverrides = new();
    private List<RoleEditor> injections;


    /// <summary>
    /// Utilized for "live" instances of the class AKA when the game is actually being played
    /// </summary>
    /// <returns>Shallow clone of this class (except for certain fields such as roleOptions being a deep clone)</returns>
    public CustomRole Instantiate(PlayerControl player)
    {
        CustomRole cloned = Clone();
        cloned.MyPlayer = player;

        if (cloned.Editor != null)
            cloned.Editor = cloned.Editor.Instantiate(cloned, player);

        CreateInstanceBasedVariables();
        cloned.Setup(player);
        cloned.SetupUI2(player.NameModel());
        player.NameModel().Render(force: true);
        if (StaticOptions.AllRolesCanVent && cloned.VirtualRole == RoleTypes.Crewmate)
            cloned.VirtualRole = RoleTypes.Engineer;

        cloned.PostSetup();
        return cloned;
    }

    public CustomRole Clone()
    {
        CustomRole cloned = (CustomRole)this.MemberwiseClone();
        cloned.roleSpecificGameOptionOverrides = new();
        cloned.currentOverrides = new();
        cloned.Modify(new RoleModifier(cloned));
        return cloned;
    }

    public bool IsEnabled() => this.Chance > 0 && this.Count > 0;

    /// <summary>
    /// Adds a GameOverride that continuously modifies this instances game options until removed
    /// </summary>
    /// <param name="optionOverride">Override to apply whenever SyncOptions is called</param>
    public void AddOverride(GameOptionOverride optionOverride) => currentOverrides.Add(optionOverride);
    /// <summary>
    /// Removes a continuous GameOverride
    /// </summary>
    /// <param name="optionOverride">Override to remove</param>
    protected void RemoveOverride(GameOptionOverride optionOverride) => currentOverrides.Remove(optionOverride);
    /// <summary>
    /// Removes a continuous GameOverride
    /// </summary>
    /// <param name="override">Override type to remove</param>
    protected void RemoveOverride(Override @override) => currentOverrides.RemoveWhere(o => o.Option == @override);

    // Useful for shorthand delegation
    public void SyncOptions() => SyncOptions(null);

    public void SyncOptions(IEnumerable<GameOptionOverride> newOverrides = null)
    {
        if (MyPlayer == null || !AmongUsClient.Instance.AmHost) return;
        List<GameOptionOverride> thisList = new(currentOverrides);

        thisList.AddRange(this.roleSpecificGameOptionOverrides);
        if (newOverrides != null) thisList.AddRange(newOverrides);

        thisList.StrJoin().DebugLog($"Sending Overrides To {MyPlayer.GetNameWithRole()}: ");

        DesyncOptions.SendModifiedOptions(thisList, MyPlayer);
    }


    public void Assign(bool desync = false)
    {
        // Here we do a "lazy" check for (all?) conditions that'd cause a role to need to be desync
        if (this.DesyncRole != null || this is Impostor)
        {

            // Get the ACTUAL role to assign the player
            RoleTypes assignedType = this.DesyncRole ?? this.VirtualRole;
            // Get the opposite type of this role
            if (MyPlayer.IsHost())
            {
                MyPlayer.SetRole(assignedType); // Required because the rpc below doesn't target host
            }
            else
            {
                // Send information to client about their new role
                VentLogger.Old($"Sending role ({assignedType}) information to {MyPlayer.UnalteredName()}", "");
                RpcV2.Immediate(MyPlayer.NetId, (byte)RpcCalls.SetRole).Write((ushort)assignedType).Send(MyPlayer.GetClientId());
            }

            // Determine roles that are "allied" with this one(based on method overrides)
            PlayerControl[] allies = Game.GetAllPlayers().Where(p => Relationship(p) is Relation.FullAllies || p.PlayerId == MyPlayer.PlayerId).ToArray();
            int[] alliesCID = allies.Select(p => p.GetClientId()).ToArray();

            allies.Select(player => player.UnalteredName()).StrJoin().DebugLog($"{this.RoleName}'s allies are: ");

            int[] crewmateReceivers = Game.GetAllPlayers()
                .Where(p => p.GetCustomRole().RealRole.IsCrewmate())
                .Select(p => p.GetClientId()).ToArray();

            //int[] allies = allies.Where(ally => ally.is)
            // Send to all clients, excluding allies, that you're a crewmate
            RpcV2.Immediate(MyPlayer.NetId, (byte)RpcCalls.SetRole).Write((ushort)RoleTypes.Impostor).SendInclusive(include: crewmateReceivers);

            RpcV2.Immediate(MyPlayer.NetId, (byte)RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).SendExclusive(exclude: alliesCID.Union(crewmateReceivers).ToArray());
            // Send to allies your real role
            RpcV2.Immediate(MyPlayer.NetId, (byte)RpcCalls.SetRole).Write((ushort)assignedType).SendInclusive(include: alliesCID);
            // Finally, for all players that are not your allies make them crewmates
            Game.GetAllPlayers()
                .Where(pc => !alliesCID.Contains(pc.GetClientId()) && pc.PlayerId != MyPlayer.PlayerId)
                .Do(pc => RpcV2.Immediate(pc.NetId, (byte)RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).Send(MyPlayer.GetClientId()));
            ShowRoleToTeammates(allies);

            if (MyPlayer.IsHost())
                Game.GetAlivePlayers().Except(allies).Do(p => p.Data.Role.Role = RoleTypes.Crewmate);
        }
        else
            MyPlayer.RpcSetRole(this.VirtualRole);
        HudManager.Instance.SetHudActive(true);
    }

    private void ShowRoleToTeammates(IEnumerable<PlayerControl> allies)
    {
        // Currently only impostors can show each other their roles
        if (!this.Faction.AlliesSeeRole()) return;
        List<PlayerControl> viewers = allies.ToList();
        MyPlayer.NameModel().GetComponentHolder<RoleHolder>()[0].SetViewerSupplier(() => viewers);
    }

    private void SetupUI2(INameModel nameModel)
    {
        GameState[] gameStates = { GameState.InIntro, GameState.Roaming, GameState.InMeeting };

        if (this is Subrole subrole) nameModel.GetComponentHolder<SubroleHolder>().Add(new SubroleComponent(subrole, gameStates, viewers: MyPlayer));
        nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(this, gameStates, ViewMode.Replace, MyPlayer));
        SetupUiFields(nameModel);
        SetupUiMethods(nameModel);
    }

    private void SetupUiFields(INameModel nameModel)
    {
        GameState[] gameStates = { GameState.InIntro, GameState.Roaming, GameState.InMeeting };
        this.GetType().GetFields(AccessFlags.InstanceAccessFlags)
            .Where(f => f.GetCustomAttribute<UIComponent>() != null)
            .Reverse()
            .ForEach(f =>
            {
                UIComponent uiComponent = f.GetCustomAttribute<UIComponent>()!;
                object? value = f.GetValue(this);
                switch (uiComponent.Component)
                {
                    case UI.Name:
                        if (value is not string s) throw new ArgumentException($"Values for \"{nameof(UI.Name)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(s, gameStates, viewers: MyPlayer));
                        break;
                    case UI.Role:
                        if (value is not CustomRole cr) throw new ArgumentException($"Values for \"{nameof(UI.Role)}\" must be {nameof(CustomRole)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(cr, gameStates, viewers: MyPlayer));
                        break;
                    case UI.Subrole:
                        if (value is not Subrole sr) throw new ArgumentException($"Values for \"{nameof(UI.Subrole)}\" must be {nameof(Subrole)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<SubroleHolder>().Add(new SubroleComponent(sr, gameStates, viewers: MyPlayer));
                        break;
                    case UI.Cooldown:
                        if (value is not Cooldown cd) throw new ArgumentException($"Values for \"{nameof(UI.Cooldown)}\" must be {nameof(Cooldown)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        VentLogger.Fatal($"Loading Cooldown Field: {cd} for {this}");
                        nameModel.GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(cd, gameStates, viewers: MyPlayer));
                        break;
                    case UI.Counter:
                        if (value is not ICounter counter) throw new ArgumentException($"Values for \"{nameof(UI.Counter)}\" must be {nameof(ICounter)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(counter, gameStates, viewers: MyPlayer));
                        break;
                    case UI.Indicator:
                        if (value is not string ind) throw new ArgumentException($"Values for \"{nameof(UI.Indicator)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(ind, gameStates, viewers: MyPlayer));
                        break;
                    case UI.Text:
                        if (value is not string txt) throw new ArgumentException($"Values for \"{nameof(UI.Indicator)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<TextHolder>().Add(new TextComponent(txt, gameStates, viewers: MyPlayer));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Component: {uiComponent.Component} is not a valid component in role: {EnglishRoleName}");
                }
            });
    }

    private void SetupUiMethods(INameModel nameModel)
    {
        GameState[] gameStates = { GameState.InIntro, GameState.Roaming, GameState.InMeeting };
        this.GetType().GetMethods(AccessFlags.InstanceAccessFlags)
            .Where(m => m.GetCustomAttribute<UIComponent>() != null)
            .Reverse()
            .ForEach(m =>
            {
                UIComponent uiComponent = m.GetCustomAttribute<UIComponent>()!;
                if (m.GetParameters().Length > 0) throw new ConstraintException($"Methods marked by {nameof(UIComponent)} must have no parameters");

                Func<string> supplier = () => m.Invoke(this, null)?.ToString() ?? "N/A";
                switch (uiComponent.Component)
                {
                    case UI.Name:
                        nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(new LiveString(supplier), gameStates, viewers: MyPlayer));
                        break;
                    case UI.Indicator:
                        nameModel.GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString(supplier), gameStates, viewers: MyPlayer));
                        break;
                    case UI.Text:
                        nameModel.GetComponentHolder<TextHolder>().Add(new TextComponent(new LiveString(supplier), gameStates, viewers: MyPlayer));
                        break;
                    case UI.Cooldown:
                        object? CooldownSupplier() => m.Invoke(this, null);
                        var obj = CooldownSupplier();
                        if (obj is not Cooldown) throw new ArgumentException($"Values for \"{nameof(UI.Cooldown)}\" must be {nameof(Cooldown)}. (Got: {obj?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(() => (Cooldown)m.Invoke(this, null)!, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Counter:
                        object? CounterSupplier() => m.Invoke(this, null);
                        var counterObj = CounterSupplier();
                        if (counterObj is string)
                        {
                            nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(new LiveString(() => (string)m.Invoke(this, null)!), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                            break;
                        }
                        if (counterObj is not ICounter) throw new ArgumentException($"Values for \"{nameof(UI.Counter)}\" must be {nameof(ICounter)}. (Got: {counterObj?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(() => (ICounter)m.Invoke(this, null)!, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Role:
                    case UI.Subrole:
                    default:
                        throw new ArgumentOutOfRangeException($"Component: {uiComponent.Component} is not a valid component");
                }
            });
    }


    public CustomRole Read(MessageReader reader)
    {
        return CustomRoleManager.GetRoleFromId(reader.ReadInt32());
    }

    public void Write(MessageWriter writer)
    {
        writer.Write(CustomRoleManager.GetRoleId(this));
    }


    public static bool operator ==(CustomRole? a, CustomRole? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(CustomRole a, CustomRole b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CustomRole role) return false;
        return role.GetType() == this.GetType();
    }
}