using System.Collections.Generic;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.Logging;
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
            Hook? hook = TemplateTriggers.BindTrigger(key, ev, tr => RunTemplateCallback(ev, tr));
            if (hook != null)
            {
                VentLogger.Trace($"Successfully bound Template Trigger \"{key}\" for \"{ev}\"", "TemplateTriggers");
                BoundHooks[key] = hook;
            } else VentLogger.Trace($"Could not bind Template Trigger \"{key}\" for \"{ev}\"", "TemplateTriggers");
        });
    }

    private void RunTemplateCallback(string triggerName, ResolvedTrigger? result)
    {
        Profiler.Sample sample = Profilers.Global.Sampler.Sampled("TriggerCallback");
        if (result == null) VentLogger.Warn($"Unable to run call back for \"{triggerName}\". No valid resolvers found for trigger event.");
        else
        {
            MetaVariable = result.Data;
            DevLogger.Log($"Meta Variable: {MetaVariable}");
            if (result.Player != null) Triggerer = result.Player.PlayerId;

            DevLogger.Log($"Result: {Evaluate(result.Player)}");
            if (Evaluate(result.Player))
                Players.GetPlayers().ForEach(p => Parent.SendMessage(p, p, p));

            MetaVariable = null;
            Triggerer = byte.MaxValue;
        }
        sample.Stop();
    }
}
