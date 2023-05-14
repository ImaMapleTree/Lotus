using System;
using Lotus.API.Reactive;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Patches.Systems;
using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew.Potions;

public class PotionSeeing: Potion
{
    [Localized("Sight")]
    public static string PotionName = "Potion of Sight";

    private string randomId = Guid.NewGuid().ToString();

    public override string Name() => PotionName;

    public PotionSeeing(int requiredCatalyst) : base((1, Ingredient.Sight), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override Color Color() => UnityEngine.Color.cyan;

    public override bool Use(PlayerControl user)
    {
        Alchemist alchemist = user.GetCustomRole<Alchemist>();
        if (SabotagePatch.CurrentSabotage != null && SabotageVariant(alchemist, SabotagePatch.CurrentSabotage)) return true;
        alchemist.VisionMod += 0.5f;
        alchemist.SyncOptions();
        Async.Schedule(() =>
        {
            alchemist.VisionMod -= 0.5f;
            alchemist.SyncOptions();
        }, 60f);
        return true;
    }

    private bool SabotageVariant(Alchemist alchemist, ISabotage sabotage)
    {
        if (sabotage.SabotageType() is not SabotageType.Lights) return false;
        alchemist.VisionMod *= 5;
        alchemist.SyncOptions();
        Hooks.SabotageHooks.SabotageFixedHook.Bind(randomId, _ => EndSabotageEffect(alchemist));
        return true;
    }

    private void EndSabotageEffect(Alchemist alchemist)
    {
        alchemist.VisionMod /= 5;
        alchemist.SyncOptions();
        Hooks.SabotageHooks.SabotageFixedHook.Unbind(randomId);
    }
}