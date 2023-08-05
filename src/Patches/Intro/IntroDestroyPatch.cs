using System.Collections;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using Lotus.Server;
using LotusTrigger.Options;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;
using static VentLib.Utilities.Debug.Profiling.Profilers;

namespace Lotus.Patches.Intro;


[HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
class IntroDestroyPatch
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(IntroDestroyPatch));

    public static void Postfix(IntroCutscene __instance)
    {
        Profiler.Sample destroySample = Global.Sampler.Sampled();
        Game.State = GameState.Roaming;
        if (!AmongUsClient.Instance.AmHost) return;

        string pet = GeneralOptions.MiscellaneousOptions.AssignedPet;
        while (pet == "Random") pet = ModConstants.Pets.Values.ToList().GetRandom();

        Profiler.Sample fullSample = Global.Sampler.Sampled("Setup ALL Players");
        Players.GetPlayers().ForEach(p =>
        {
            Profiler.Sample executeSample = Global.Sampler.Sampled("Execution Pregame Setup");
            Async.Execute(ServerPatchManager.Patch.ExecuteT<IEnumerator>(PatchedCode.PreGameSetup, p, pet));
            executeSample.Stop();
        });
        fullSample.Stop();

        Profiler.Sample propSample = Global.Sampler.Sampled("Propagation Sample");
        log.Trace("Intro Scene Ending", "IntroCutscene");
        ActionHandle handle = ActionHandle.NoInit();
        Game.TriggerForAll(LotusActionType.RoundStart, ref handle, true);
        propSample.Stop();

        Hooks.GameStateHooks.RoundStartHook.Propagate(new GameStateHookEvent(Game.MatchData));
        destroySample.Stop();
    }
}