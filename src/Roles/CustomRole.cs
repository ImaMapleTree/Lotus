using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
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
using VentLib.Networking;
using VentLib.Networking.Interfaces;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Roles;

public abstract class CustomRole : AbstractBaseRole, IRpcSendable<CustomRole>
{
    static CustomRole()
    {
        AbstractConstructors.Register(typeof(CustomRole), r => CustomRoleManager.GetRoleFromId(r.ReadInt32()));
    }


    public virtual bool CanVent() => BaseCanVent || GeneralOptions.MayhemOptions.AllRolesCanVent;
    public virtual Relation Relationship(PlayerControl player) => this.Relationship(player.GetCustomRole());

    private RemoteList<GameOptionOverride> currentOverrides = new();
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
        if (GeneralOptions.MayhemOptions.AllRolesCanVent && cloned.VirtualRole == RoleTypes.Crewmate)
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
    public Remote<GameOptionOverride> AddOverride(GameOptionOverride optionOverride) => currentOverrides.Add(optionOverride);

    public GameOptionOverride? GetOverride(Override overrideType) => currentOverrides.FirstOrDefault(o => o.Option == overrideType);
    /// <summary>
    /// Removes a continuous GameOverride
    /// </summary>
    /// <param name="optionOverride">Override to remove</param>
    protected void RemoveOverride(GameOptionOverride optionOverride) => currentOverrides.Remove(optionOverride);
    /// <summary>
    /// Removes a continuous GameOverride
    /// </summary>
    /// <param name="override">Override type to remove</param>
    protected void RemoveOverride(Override @override) => currentOverrides.RemoveAll(o => o.Option == @override);

    // Useful for shorthand delegation
    public void SyncOptions() => SyncOptions(null);

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public void SyncOptions(IEnumerable<GameOptionOverride>? newOverrides = null, bool official = false)
    {
        if (MyPlayer == null || !AmongUsClient.Instance.AmHost) return;
        List<GameOptionOverride> thisList = new(currentOverrides);

        thisList.AddRange(this.roleSpecificGameOptionOverrides);
        if (newOverrides != null) thisList.AddRange(newOverrides);

        IGameOptions modifiedOptions = DesyncOptions.GetModifiedOptions(thisList);
        if (official) RpcV3.Immediate(PlayerControl.LocalPlayer.NetId, RpcCalls.SyncSettings).Write(modifiedOptions).Send(MyPlayer.GetClientId());
        DesyncOptions.SyncToPlayer(modifiedOptions, MyPlayer);
    }


    public void Assign()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        bool isStartOfGame = Game.State is GameState.InIntro or GameState.InLobby;

        if (RealRole.IsCrewmate())
        {
            MyPlayer.RpcSetRole(RealRole);

            if (!isStartOfGame) goto finishAssignment;

            Game.GetAllPlayers().ForEach(p => p.GetTeamInfo().AddPlayer(MyPlayer.PlayerId, MyPlayer.GetVanillaRole().IsImpostor()));

            goto finishAssignment;
        }


        VentLogger.Trace($"Setting {MyPlayer.name} Role => {RealRole} | IsStartGame = {isStartOfGame}", "CustomRole::Assign");
        if (MyPlayer.IsHost()) MyPlayer.SetRole(RealRole);
        else RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RealRole).Send(MyPlayer.GetClientId());

        PlayerControl[] alliedPlayers = Game.GetAllPlayers().Where(p => Relationship(p) is Relation.FullAllies).ToArray();
        VentLogger.Debug($"Player {MyPlayer.GetNameWithRole()} Allies: [{alliedPlayers.Select(p => p.name).Fuse()}]");
        HashSet<byte> alliedPlayerIds = alliedPlayers.Select(p => p.PlayerId).ToHashSet();
        int[] alliedPlayerClientIds = alliedPlayers.Select(p => p.GetClientId()).ToArray();

        PlayerControl[] crewmates = Game.GetAllPlayers().Where(p => p.GetVanillaRole().IsCrewmate()).ToArray();
        int[] crewmateClientIds = crewmates.Select(p => p.GetClientId()).ToArray();
        VentLogger.Trace($"Current Crewmates: [{crewmates.Select(p => p.name).Fuse()}]");

        PlayerControl[] nonAlliedImpostors = Game.GetAllPlayers().Where(p => p.GetVanillaRole().IsImpostor()).Where(p => !alliedPlayerIds.Contains(p.PlayerId)).ToArray();
        int[] nonAlliedImpostorClientIds = nonAlliedImpostors.Select(p => p.GetClientId()).ToArray();
        VentLogger.Trace($"Non Allied Impostors: [{nonAlliedImpostors.Select(p => p.name).Fuse()}]");

        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RealRole).SendInclusive(alliedPlayerClientIds);
        if (isStartOfGame) alliedPlayers.ForEach(p => p.GetTeamInfo().AddPlayer(MyPlayer.PlayerId, RealRole.IsImpostor()));

        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RoleTypes.Crewmate).SendInclusive(nonAlliedImpostorClientIds);
        if (isStartOfGame) nonAlliedImpostors.ForEach(p => p.GetTeamInfo().AddVanillaCrewmate(MyPlayer.PlayerId));

        RpcV3.Immediate(MyPlayer.NetId, RpcCalls.SetRole).Write((ushort)RoleTypes.Impostor).SendInclusive(crewmateClientIds);
        if (isStartOfGame) crewmates.ForEach(p => p.GetTeamInfo().AddVanillaImpostor(MyPlayer.PlayerId));

        ShowRoleToTeammates(alliedPlayers);

        if (MyPlayer.IsHost()) Game.GetAlivePlayers().Except(alliedPlayers).ForEach(p => p.Data.Role.Role = RoleTypes.Crewmate);

        finishAssignment:

        // This is for host
        if (MyPlayer.Relationship(PlayerControl.LocalPlayer) is Relation.FullAllies) MyPlayer.SetRole(RealRole);
        else MyPlayer.SetRole(RealRole.IsImpostor() ? RoleTypes.Crewmate : RoleTypes.Impostor);

        SyncOptions(new GameOptionOverride[] { new(Override.KillCooldown, 0.1f)} , true);
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
                        nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(s, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Role:
                        if (value is not CustomRole cr) throw new ArgumentException($"Values for \"{nameof(UI.Role)}\" must be {nameof(CustomRole)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(cr, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Subrole:
                        if (value is not Subrole sr) throw new ArgumentException($"Values for \"{nameof(UI.Subrole)}\" must be {nameof(Subrole)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<SubroleHolder>().Add(new SubroleComponent(sr, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Cooldown:
                        if (value is not Cooldown cd) throw new ArgumentException($"Values for \"{nameof(UI.Cooldown)}\" must be {nameof(Cooldown)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        VentLogger.Fatal($"Loading Cooldown Field: {cd} for {this}");
                        nameModel.GetComponentHolder<CooldownHolder>().Add(new CooldownComponent(cd, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Counter:
                        if (value is not ICounter counter) throw new ArgumentException($"Values for \"{nameof(UI.Counter)}\" must be {nameof(ICounter)}. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<CounterHolder>().Add(new CounterComponent(counter, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Indicator:
                        if (value is not string ind) throw new ArgumentException($"Values for \"{nameof(UI.Indicator)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(ind, uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Text:
                        if (value is not string txt) throw new ArgumentException($"Values for \"{nameof(UI.Indicator)}\" must be string. (Got: {value?.GetType()}) in role: {EnglishRoleName}");
                        nameModel.GetComponentHolder<TextHolder>().Add(new TextComponent(txt, uiComponent.GameStates, uiComponent.ViewMode,  viewers: MyPlayer));
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
                        nameModel.GetComponentHolder<NameHolder>().Add(new NameComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Role:
                        nameModel.GetComponentHolder<RoleHolder>().Add(new RoleComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Indicator:
                        nameModel.GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
                        break;
                    case UI.Text:
                        nameModel.GetComponentHolder<TextHolder>().Add(new TextComponent(new LiveString(supplier), uiComponent.GameStates, uiComponent.ViewMode, viewers: MyPlayer));
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

    public static bool operator !=(CustomRole? a, CustomRole? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CustomRole role) return false;
        return role.GetType() == this.GetType();
    }
}