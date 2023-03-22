using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TOHTOR.API;
using TOHTOR.GUI.Name.Components;
using TOHTOR.GUI.Name.Holders;
using TOHTOR.GUI.Name.Interfaces;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace TOHTOR.GUI.Name.Impl;

public class SimpleNameModel : INameModel
{
    private string unalteredName;
    private int spacing = 0;

    private string cacheString = "";

    public NameHolder NameHolder = new NameHolder();
    public IndicatorHolder IndicatorHolder = new IndicatorHolder();
    public RoleHolder RoleHolder = new RoleHolder(1);
    public SubroleHolder SubroleHolder = new SubroleHolder(1);
    public CounterHolder CounterHolder = new CounterHolder(1);
    public CooldownHolder CooldownHolder = new CooldownHolder(2);
    public TextHolder TextHolder = new TextHolder(2);

    private List<IComponentHolder> componentHolders;
    private PlayerControl player;
    private Dictionary<byte, DateTime> renders = new();

    private void SetHolders()
    {
        componentHolders = new()
        {
            NameHolder, IndicatorHolder,
            RoleHolder, SubroleHolder, CounterHolder,
            CooldownHolder, TextHolder
        };
    }

    public SimpleNameModel(PlayerControl player)
    {
        this.player = player;
        SetHolders();
        this.unalteredName = player.Data.PlayerName;
        NameHolder.Add(new NameComponent(new LiveString(unalteredName, Color.white), new[] { GameState.Roaming, GameState.InMeeting}, ViewMode.Replace));
    }

    public string Unaltered() => unalteredName;

    public PlayerControl MyPlayer() => player;

    public string Render(GameState? state = null, bool sendToPlayer = true, bool force = false)
    {
        return this.RenderFor(MyPlayer(), state, sendToPlayer, force);
    }

    public string RenderFor(PlayerControl rPlayer, GameState? state = null, bool sendToPlayer = true, bool force = false)
    {
        float durationSinceLast = (float)(DateTime.Now - this.renders.GetOrCompute(rPlayer.PlayerId, () => DateTime.Now)).TotalSeconds;
        if (!force && durationSinceLast < ModConstants.DynamicNameTimeBetweenRenders && Game.State is not GameState.InMeeting) return cacheString;
        this.renders[rPlayer.PlayerId] = DateTime.Now;

        state ??= Game.State;
        List<List<string>> renders = new();
        bool updated = false;
        foreach (IComponentHolder componentHolder in ComponentHolders())
        {
            for (int i = 0; i < componentHolder.Line() + 1 - renders.Count; i++) renders.Add(new List<string>());
            renders[componentHolder.Line()].Add(componentHolder.Render(rPlayer, state.Value));
            updated = updated || componentHolder.Updated(rPlayer.PlayerId);
        }

        if (!updated && !force) return cacheString;
        cacheString = renders.Select(s => s.Join(delimiter: " ".Repeat(spacing - 1))).Join(delimiter: "\n").TrimStart('\n').TrimEnd('\n');
        if (sendToPlayer) RpcV2.Immediate(player.NetId, RpcCalls.SetName).Write(cacheString).Send(rPlayer.GetClientId());
        return cacheString;
    }

    public List<IComponentHolder> ComponentHolders() => componentHolders;

    public T GetComponentHolder<T>() where T : IComponentHolder
    {
        return (T)componentHolders.First(f => f is T);
    }
}