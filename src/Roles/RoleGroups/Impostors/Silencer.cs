using TOHTOR.Roles.RoleGroups.Vanilla;

namespace TOHTOR.Roles.RoleGroups.Impostors;

public class Silencer: Impostor
{
    /*private static string _blackmailedMessage = Localizer.Translate("Roles.Blackmail.BlackmailMessage");
    private static string _warningMessage = Localizer.Translate("Roles.Blackmail.BlackmailWarning");
    private Remote<TextComponent>? blackmailingText;
    private Optional<PlayerControl> blackmailedPlayer = Optional<PlayerControl>.Null();

    private bool showBlackmailedToAll;

    private int warnsUntilKick;
    private int currentWarnings;

    [RoleAction(RoleActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(RoleActionType.Shapeshift)]
    public void Blackmail(PlayerControl target, ActionHandle handle)
    {
        if (target.PlayerId == MyPlayer.PlayerId) return;
        handle.Cancel();
        blackmailingText?.Delete();
        blackmailedPlayer = Optional<PlayerControl>.NonNull(target);
        TextComponent textComponent = new(new LiveString("BLACKMAILED", Color.red), GameStates.IgnStates, viewers: MyPlayer);
        blackmailingText = target.NameModel().GetComponentHolder<TextHolder>().Add(textComponent);
    }

    [RoleAction(RoleActionType.RoundStart)]
    public void ClearBlackmail()
    {
        blackmailedPlayer = Optional<PlayerControl>.Null();
        currentWarnings = 0;
        blackmailingText?.Delete();
    }

    [RoleAction(RoleActionType.RoundEnd)]
    public void NotifyBlackmailed()
    {
        List<PlayerControl> allPlayers = showBlackmailedToAll
            ? Game.GetAllPlayers().ToList()
            : blackmailedPlayer.Transform(p => new List<PlayerControl> { p, MyPlayer }, () => new List<PlayerControl> { MyPlayer });
        blackmailingText?.Get()?.SetViewerSupplier(() => allPlayers);
        blackmailedPlayer.IfPresent(p =>
        {
            string message = $"{RoleColor.Colorize(MyPlayer.UnalteredName())} blackmailed {p.GetRoleColor().Colorize(p.UnalteredName())}.";
            Game.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, p, message));
            Utils.SendMessage(_blackmailedMessage, p.PlayerId);
        });
    }

    [RoleAction(RoleActionType.Chat)]
    public void InterceptChat(PlayerControl speaker, GameState state, bool isAlive)
    {
        if (!isAlive || state is not GameState.InMeeting) return;
        if (!blackmailedPlayer.Exists() || speaker.PlayerId != blackmailedPlayer.Get().PlayerId) return;
        if (currentWarnings++ < warnsUntilKick)
        {
            Utils.SendMessage(_warningMessage, speaker.PlayerId);
            return;
        }

        VentLogger.Trace($"Blackmailer Killing Player: {speaker.UnalteredName()}");
        MyPlayer.InteractWith(speaker, new UnblockedInteraction(new FatalIntent(), this));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Warnings Until Death")
                .AddIntRange(0, 5, 1)
                .BindInt(i => warnsUntilKick = i)
                .Build())
            .SubOption(sub => sub.Name("Show Blackmailed to All")
                .AddOnOffValues()
                .BindBool(b => showBlackmailedToAll = b)
                .Build());*/
}