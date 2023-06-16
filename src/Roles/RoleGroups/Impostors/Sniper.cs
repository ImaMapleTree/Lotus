using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles.RoleGroups.Impostors;

public class Sniper: Shapeshifter
{
    private bool preciseShooting;
    private int playerPiercing;
    private bool refundOnKill;

    private int totalBulletCount;
    private int currentBulletCount;
    private Vector2 startingLocation;

    [UIComponent(UI.Counter)]
    private string BulletCountCounter() => currentBulletCount >= 0 ? RoleUtils.Counter(currentBulletCount, color: ModConstants.Palette.MadmateColor) : "";

    protected override void PostSetup()
    {
        currentBulletCount = totalBulletCount;
        ShapeshiftDuration = 5f;
    }

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        bool success = currentBulletCount == 0 && base.TryKill(target);
        if (success && refundOnKill && currentBulletCount >= 0) currentBulletCount++;
        return success;
    }

    [RoleAction(RoleActionType.Shapeshift)]
    private void StartSniping()
    {
        startingLocation = MyPlayer.GetTruePosition();
        DevLogger.Log($"Starting position: {startingLocation}");
    }

    [RoleAction(RoleActionType.Unshapeshift)]
    private bool FireBullet()
    {
        if (currentBulletCount == 0) return false;
        currentBulletCount--;

        Vector2 targetPosition = (MyPlayer.GetTruePosition() - startingLocation).normalized;
        DevLogger.Log($"Target Position: {targetPosition}");
        int kills = 0;

        foreach (PlayerControl target in Game.GetAllPlayers().Where(p => p.PlayerId != MyPlayer.PlayerId && p.Relationship(MyPlayer) is not Relation.FullAllies))
        {
            DevLogger.Log(target.name);
            Vector3 targetPos = target.transform.position - (Vector3)MyPlayer.GetTruePosition();
            Vector3 targetDirection = targetPos.normalized;
            DevLogger.Log($"Target direction: {targetDirection}");
            float dotProduct = Vector3.Dot(targetPosition, targetDirection);
            DevLogger.Log($"Dot Product: {dotProduct}");
            float error = !preciseShooting ? targetPos.magnitude : Vector3.Cross(targetPosition, targetPos).magnitude;
            DevLogger.Log($"Error: {error}");
            if (dotProduct < 0.98 || (error >= 1.0 && preciseShooting)) continue;
            float distance = Vector2.Distance(MyPlayer.transform.position, target.transform.position);
            InteractionResult result = MyPlayer.InteractWith(target, new RangedInteraction(new FatalIntent(true), distance, this));
            if (result is InteractionResult.Halt) continue;
            kills++;
            MyPlayer.RpcMark();
            if (kills > playerPiercing && playerPiercing != -1) break;
        }

        if (kills > 0 && refundOnKill) currentBulletCount++;

        return kills > 0;
    }


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Sniper Bullet Count", Translations.Options.SniperBulletCount)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .BindInt(v => totalBulletCount = v)
                .SubOption(sub2 => sub2
                    .KeyName("Refund Bullet on Kills", Translations.Options.RefundBulletOnKill)
                    .BindBool(b => refundOnKill = b)
                    .AddOnOffValues(false)
                    .Build())
                .AddIntRange(1, 20, 1, 8)
                .Build())
            .SubOption(sub => sub
                .KeyName("Sniping Cooldown", Translations.Options.SnipingCooldown)
                .BindFloat(f => ShapeshiftCooldown = f + 5f)
                .AddFloatRange(2.5f, 120f, 2.5f, 19, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Precise Shooting", Translations.Options.PreciseShooting)
                .BindBool(v => preciseShooting = v)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .KeyName("Player Piercing", Translations.Options.PlayerPiercing)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .BindInt(v => playerPiercing = v)
                .AddIntRange(1, 15, 1, 2)
                .Build());

    [Localized(nameof(Sniper))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(SniperBulletCount))]
            public static string SniperBulletCount = "Sniper Bullet Count";

            [Localized(nameof(SnipingCooldown))]
            public static string SnipingCooldown = "Sniping Cooldown";

            [Localized(nameof(RefundBulletOnKill))]
            public static string RefundBulletOnKill = "Refund Bullet on Kills";

            [Localized(nameof(PreciseShooting))]
            public static string PreciseShooting = "Precise Shooting";

            [Localized(nameof(PlayerPiercing))]
            public static string PlayerPiercing = "Player Piercing";
        }
    }

}