using TOHTOR.API;
using TOHTOR.Extensions;
using TOHTOR.GUI;
using TOHTOR.GUI.Name;
using TOHTOR.Options;
using TOHTOR.Roles.Internals;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Victory;
using TOHTOR.Victory.Conditions;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Roles.RoleGroups.Neutral;

public class Survivor : CustomRole
{
    [UIComponent(UI.Cooldown)]
    private Cooldown vestCooldown;
    private Cooldown vestDuration;

    private int vestUsages;
    private int reaminingVests;

    [UIComponent(UI.Counter)]
    private string VestCounter() => RoleUtils.Counter(reaminingVests, vestUsages, RoleColor);

    [UIComponent(UI.Indicator)]
    private string GetVestString() => vestDuration.IsReady() ? "" : RoleColor.Colorize("♣");

    public override bool CanBeKilled() => vestDuration.IsReady();

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        vestDuration.Start(10f);
        reaminingVests = vestUsages;
        Game.GetWinDelegate().AddSubscriber(GameEnd);
    }

    [RoleAction(RoleActionType.OnPet)]
    public void OnPet()
    {
        if (reaminingVests == 0 || vestDuration.NotReady() || vestCooldown.NotReady()) return;
        reaminingVests--;
        vestDuration.StartThenRun(() => vestCooldown.Start());;
    }

    private void GameEnd(WinDelegate winDelegate)
    {
        if (!MyPlayer.IsAlive() || winDelegate.GetWinReason() is WinReason.SoloWinner) return;
        winDelegate.GetWinners().Add(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.NeutralTab)
            .SubOption(sub => sub
                .Name("Vest Duration")
                .BindFloat(vestDuration.SetDuration)
                .AddFloatRange(2.5f, 180f, 2.5f, 11, "s")
                .Build())
            .SubOption(sub => sub
                .Name("Vest Cooldown")
                .BindFloat(vestCooldown.SetDuration)
                .AddFloatRange(2.5f, 180f, 2.5f, 5, "s")
                .Build())
            .SubOption(sub => sub.Name("Vest Usages")
                .BindInt(i => vestUsages = i)
                .Value(v => v.Value(-1).Text("∞").Build())
                .AddIntRange(1, 60, 1)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .SpecialType(SpecialType.Neutral)
            .RoleColor("#FFE64D");
}