using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Utilities;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using Lotus.Options;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew;

public class ExConvict: Crewmate
{
    private Vector2? location;
    private bool clearMarkAfterMeeting;


    [UIComponent(UI.Cooldown)]
    private Cooldown canEscapeCooldown;

    [UIComponent(UI.Cooldown)]
    private Cooldown canMarkCooldown;

    [UIComponent(UI.Text)]
    private string TpIndicator() => canEscapeCooldown.IsReady() && location != null ? Color.cyan.Colorize("Press Pet to Escape") : "";

    protected override void PostSetup()
    {
        CooldownHolder cooldownHolder = MyPlayer.NameModel().GetComponentHolder<CooldownHolder>();
        cooldownHolder.Get(1).SetPrefix("Escape In: ").SetTextColor(Color.cyan);
    }

    [RoleAction(RoleActionType.OnPet)]
    private void PetAction()
    {
        if (location == null) TryMarkLocation();
        else TryEscape();
    }

    [RoleAction(RoleActionType.RoundStart)]
    private void ClearMark()
    {
        if (clearMarkAfterMeeting) location = null;
    }

    private void TryMarkLocation()
    {
        if (canMarkCooldown.NotReady()) return;
        location = MyPlayer.GetTruePosition();
        canEscapeCooldown.Start();
    }

    private void TryEscape()
    {
        if (canEscapeCooldown.NotReady() || location == null) return;
        Utils.Teleport(MyPlayer.NetTransform, location.Value);
        location = null;
        canMarkCooldown.Start();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Cooldown After Mark")
                .AddFloatRange(0, 60, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(canEscapeCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Cooldown After Escape")
                .AddFloatRange(0, 180, 2.5f, 16, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(canMarkCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Clear Mark After Meeting")
                .AddOnOffValues()
                .BindBool(b => clearMarkAfterMeeting = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.43f, 0f, 0.2f));
}