using Lotus.Extensions;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.RoleGroups.Impostors.Creeper.BomberTranslations.BomberOptionTranslations;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Creeper : Shapeshifter
{
    private bool bomberProtectedByShields;
    private float explosionRadius;

    [RoleAction(RoleActionType.OnPet)]
    [RoleAction(RoleActionType.Shapeshift)]
    private void CreeperExplode()
    {
        RoleUtils.GetPlayersWithinDistance(MyPlayer, explosionRadius).ForEach(p =>
        {
            FatalIntent intent = new(true, () => new BombedEvent(p, MyPlayer));
            MyPlayer.InteractWith(p, new DirectInteraction(intent, this));
        });
        
        FatalIntent suicideIntent = new(false, () => new BombedEvent(MyPlayer, MyPlayer));
        MyPlayer.InteractWith(MyPlayer, bomberProtectedByShields 
            ? new DirectInteraction(suicideIntent, this) 
            : new UnblockedInteraction(suicideIntent, this)
        );
    }
    
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => 
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Bomber Protected By Shielding", BomberProtection)
                .AddOnOffValues()
                .BindBool(b => bomberProtectedByShields = b)
                .Build())
            .SubOption(sub => sub.KeyName("Explosion Radius", ExplosionRadius)
                .Value(v => v.Value(1.2f).Text(SmallDistance).Build())
                .Value(v => v.Value(1.8f).Text(MediumDistance).Build())
                .Value(v => v.Value(2.4f).Text(LargeDistance).Build())
                .BindFloat(f => explosionRadius = f)
                .Build());

    [Localized(nameof(Creeper))]
    internal static class BomberTranslations
    {
        [Localized("Options")]
        internal static class BomberOptionTranslations
        {
            public static string SmallDistance = "Small";

            public static string MediumDistance = "Medium";

            public static string LargeDistance = "Large";
            
            [Localized(nameof(BomberProtection))]
            public static string BomberProtection = "Bomber Protected by Shielding";

            [Localized(nameof(ExplosionRadius))]
            public static string ExplosionRadius = "Explosion Radius";

        }
    }
}