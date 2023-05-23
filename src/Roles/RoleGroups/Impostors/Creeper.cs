using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.RoleGroups.Impostors.Creeper.CreeperTranslations.CreeperOptionTranslations;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Creeper : Shapeshifter
{
    private bool canKillNormally;
    private bool creeperProtectedByShields;
    private float explosionRadius;
    private Cooldown gracePeriod;
    
    [UIComponent(UI.Text)]
    public string GracePeriodText() => gracePeriod.IsReady() ? "" : Color.red.Colorize(CreeperTranslations.ExplosionGracePeriod).Formatted(gracePeriod + "s");

    [RoleAction(RoleActionType.RoundStart)]
    private void BeginGracePeriod()
    {
        gracePeriod.Start();
    }

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => canKillNormally && base.TryKill(target);
    
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
        MyPlayer.InteractWith(MyPlayer, creeperProtectedByShields 
            ? new DirectInteraction(suicideIntent, this) 
            : new UnblockedInteraction(suicideIntent, this)
        );
    }
    
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => 
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Can Kill Normally", CanKillNormal)
                .AddOnOffValues()
                .BindBool(b => canKillNormally = b)
                .Build())
            .SubOption(sub => sub.KeyName("Creeper Protected By Shielding", CreeperProtection)
                .AddOnOffValues()
                .BindBool(b => creeperProtectedByShields = b)
                .Build())
            .SubOption(sub => sub.KeyName("Explosion Radius", ExplosionRadius)
                .Value(v => v.Value(2f).Text(SmallDistance).Build())
                .Value(v => v.Value(3f).Text(MediumDistance).Build())
                .Value(v => v.Value(4f).Text(LargeDistance).Build())
                .BindFloat(f => explosionRadius = f)
                .Build())
            .SubOption(sub => sub.KeyName("Creeper Grace Period", CreeperGracePeriod)
                .AddFloatRange(0, 60, 2.5f, 4, "s")
                .BindFloat(gracePeriod.SetDuration)
                .Build());

    [Localized(nameof(Creeper))]
    internal static class CreeperTranslations
    {
        [Localized(nameof(ExplosionGracePeriod))]
        public static string ExplosionGracePeriod = "Explosion Grace Period: {0}";
        
        [Localized(ModConstants.Options)]
        internal static class CreeperOptionTranslations
        {
            public static string SmallDistance = "Small";

            public static string MediumDistance = "Medium";

            public static string LargeDistance = "Large";

            [Localized(nameof(CanKillNormal))]
            public static string CanKillNormal = "Can Kill Normally";

            [Localized(nameof(CreeperGracePeriod))]
            public static string CreeperGracePeriod = "Grace Period";
            
            [Localized(nameof(CreeperProtection))]
            public static string CreeperProtection = "Protected by Shielding";

            [Localized(nameof(ExplosionRadius))]
            public static string ExplosionRadius = "Explosion Radius";

        }
    }
}