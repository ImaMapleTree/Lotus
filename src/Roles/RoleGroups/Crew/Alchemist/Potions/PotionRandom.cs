using System.Linq;
using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Crew.Potions;

public class PotionRandom: Potion
{
    [Localized("Random")]
    public static string PotionName = "Unstable Concoction";

    private string gradientName = new ColorGradient(UnityEngine.Color.red, new Color(1, 0.5f, 0),
        UnityEngine.Color.yellow, UnityEngine.Color.green, new Color(0f, 0.5f, 1f),
        UnityEngine.Color.blue, new Color(0.5f, 0f, 1f)).Apply(PotionName);



    public PotionRandom(int requiredCatalyst) : base((2, Ingredient.Chaos), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => gradientName;
    public override Color Color() => UnityEngine.Color.white;

    public override bool Use(PlayerControl user)
    {
        Alchemist alchemist = user.GetCustomRole<Alchemist>();
        ICraftable randomPotion = alchemist.Craftables.Where(p => p is Potion and not PotionRandom).ToList().GetRandom();
        Async.Schedule(() => alchemist.HeldCraftable = randomPotion, 0.1f);
        return true;
    }
}