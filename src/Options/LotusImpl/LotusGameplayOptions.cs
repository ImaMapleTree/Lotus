using Lotus.API;

namespace Lotus.Options.LotusImpl;

public class LotusGameplayOptions: LotusOptionModel
{
    public ModifierTextMode ModifierTextMode;

    public bool GhostsSeeInfo;

    public bool EnableLadderDeath;
    public int LadderDeathChance;

    public virtual float GetFirstKillCooldown(PlayerControl player) => AUSettings.KillCooldown();
}

public enum ModifierTextMode
{
    First,
    Off,
    All
}