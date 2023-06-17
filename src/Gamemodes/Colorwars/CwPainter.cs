/*
using System;
using Lotus.API.Odyssey;
using Lotus.Roles;
using Lotus.Roles.Internals.Attributes;
using Lotus.Extensions;
using VentLib.Options.Game;
using static Lotus.Roles.RoleGroups.Impostors.SerialKiller;
using SerialKiller = Lotus.Roles.RoleGroups.Impostors.SerialKiller;

namespace Lotus.Gamemodes.Colorwars;

public class CwPainter: SerialKillerModifier
{
    internal CwPainter(SerialKiller role) : base(role) { }
    public override void OnLink() { }

    [ModifiedAction(RoleActionType.FixedUpdate)]
    public void FixedUpdate()
    {
        double timeElapsed = (DateTime.Now - Game.MatchData.StartTime).TotalSeconds;
        if (timeElapsed < ColorwarsGamemode.GracePeriod) {
            this.DeathTimer.Start();
            return;
        }
        CheckForSuicide();
    }

    [ModifiedAction(RoleActionType.Attack)]
    public void ColorwarsKill(PlayerControl target)
    {
        double timeElapsed = (DateTime.Now - Game.MatchData.StartTime).TotalSeconds;
        if (timeElapsed < ColorwarsGamemode.GracePeriod) return;
        if (ColorwarsGamemode.ConvertColorMode) SplatoonConvert(target);
        else {
            MyPlayer.RpcMurderPlayer(target);
            this.DeathTimer.Start();
        }
    }

    private void SplatoonConvert(PlayerControl target)
    {
        int killerColor = MyPlayer.cosmetics.bodyMatProperties.ColorId;
        if (killerColor == target.cosmetics.bodyMatProperties.ColorId) return;

        target.RpcSetColor((byte)killerColor);
        MyPlayer.RpcMark(target);
        this.DeathTimer.Start();
    }

    public override GameOptionBuilder HookOptions(GameOptionBuilder optionStream) =>
        base.HookOptions(optionStream)
            .Tab(ColorwarsGamemode.ColorwarsTab);

    public override AbstractBaseRole.RoleModifier HookModifier(AbstractBaseRole.RoleModifier modifier) =>
        base.HookModifier(modifier)
            .RoleName("Painter");
}
*/



