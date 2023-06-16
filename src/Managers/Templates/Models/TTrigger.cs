using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.Managers.Templates.Models.Units;
using VentLib.Logging;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Templates.Models;

// ReSharper disable once InconsistentNaming
public class TTrigger: TemplateUnit
{
    public static Dictionary<string, Hook> BoundHooks = new();
    public List<string>? Events { get; set; }
    public Template Parent = null!;

    public void Setup(Template parent)
    {
        Parent = parent;
        Events?.ForEach(ev =>
        {
            string key = $"{nameof(TTrigger)}~{TemplateManager.GlobalTriggerCount++}";
            VentLogger.Trace($"Binding Template Trigger \"{key}\"", "TemplateTriggers");
            Hook? hook = TemplateTriggers.BindTrigger(key, ev, tr => RunTemplateCallback(ev, tr));
            if (hook != null) BoundHooks[key] = hook;
        });
    }

    private void RunTemplateCallback(string triggerName, ResolvedTrigger? result)
    {
        Profiler.Sample sample = Profilers.Global.Sampler.Sampled("TriggerCallback");
        if (result == null) VentLogger.Warn($"Unable to run call back for \"{triggerName}\". No valid resolvers found for trigger event.");
        else
        {
            MetaVariable = result.Data;
            if (result.Player != null) Triggerer = result.Player.PlayerId;

            if (Conditions.All(c => c.Evaluate(result.Player)))
                Players.GetPlayers().ForEach(p => Parent.SendMessage(p, p, p));

            MetaVariable = null;
            Triggerer = byte.MaxValue;
        }
        sample.Stop();
    }
}
