using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;
using UnityEngine;
using VentLib.Utilities;

namespace TOHTOR.GUI.Name.Components;

public class CooldownComponent : NmComponent
{
    private Cooldown cooldown;
    private Color numberColor = Color.white;
    private Color textColor = new(0.93f, 0.57f, 0.28f);
    private Ubifix prefix;

    public CooldownComponent(Cooldown cooldown, GameState[] gameStates, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : base("", gameStates, viewMode, viewers)
    {
        this.cooldown = cooldown;
        prefix = new Ubifix(new LiveString(() => this.cooldown.NotReady() ? textColor.Colorize("CD: ") : ""));
        this.AddPrefix(prefix);
        this.SetMainText(new LiveString(TextSupplier));
    }

    public CooldownComponent(Cooldown cooldown, GameState gameState, ViewMode viewMode = Impl.ViewMode.Additive, params PlayerControl[] viewers) : this(cooldown, new []{gameState}, viewMode, viewers)
    {
    }

    public CooldownComponent SetPrefix(string prefix)
    {
        this.prefix.Delete();
        this.prefix = new Ubifix(new LiveString(() => this.cooldown.NotReady() ? textColor.Colorize(prefix) : ""));
        this.AddPrefix(this.prefix);
        return this;
    }

    public CooldownComponent SetNumberColor(Color color)
    {
        this.numberColor = color;
        return this;
    }

    public CooldownComponent SetTextColor(Color color)
    {
        this.textColor = color;
        return this;
    }

    private string TextSupplier()
    {
        string cooldownString = cooldown.IsReady() ? "" : (cooldown + "s");
        return numberColor.Colorize(cooldownString);
    }
}