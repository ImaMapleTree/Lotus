using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Hotkeys;

public class Hotkey
{
    public ulong TimesUsed { get; private set; }

    private readonly KeyCode[] keyCodes;
    private List<Func<bool>> predicates = new();
    private List<Action> actions = new();

    private Hotkey(KeyCode[] keyCodes)
    {
        this.keyCodes = keyCodes;
    }
    
    public static Hotkey When(params KeyCode[] keyCodes)
    {
        return new Hotkey(keyCodes);
    }
    

    public Hotkey If(Func<bool> predicate)
    {
        predicates.Add(predicate);
        return this;
    }

    public Hotkey If(Func<PredicateBuilder, Func<bool>> predicateBuilder)
    {
        predicates.Add(predicateBuilder(new PredicateBuilder()));
        return this;
    }
    
    public Hotkey If(Func<PredicateBuilder, PredicateBuilder> predicateBuilder)
    {
        predicates.Add(predicateBuilder(new PredicateBuilder()).Build());
        return this;
    }

    public Hotkey Do(Action action)
    {
        actions.Add(action);
        return this;
    }

    public void Update()
    {
        if (!keyCodes.Any(Input.GetKeyDown)) return;
        if (!keyCodes.All(Input.GetKey)) return;
        if (!predicates.All(p => p())) return;
        VentLogger.Trace($"HotKey Pressed ({keyCodes.Fuse()})", "HotKey::Update");
        actions.ForEach(a => a());
        TimesUsed++;
    }

    public class PredicateBuilder
    {
        private bool isHostOnly;
        private HashSet<GameState> gameStates = new();
        private Func<bool>? predicate;

        public PredicateBuilder HostOnly(bool hostOnly = true)
        {
            isHostOnly = hostOnly;
            return this;
        }

        public PredicateBuilder State(params GameState[] states)
        {
            states.ForEach(s => gameStates.Add(s));
            return this;
        }

        public PredicateBuilder Predicate(Func<bool> pred)
        {
            predicate = pred;
            return this;
        }

        public Func<bool> Build()
        {
            return () =>
            {
                if (isHostOnly && !AmongUsClient.Instance.AmHost) return false;
                if (!gameStates.IsEmpty() && !gameStates.Contains(Game.State)) return false;
                return predicate?.Invoke() ?? true;
            };
        }
    }
}