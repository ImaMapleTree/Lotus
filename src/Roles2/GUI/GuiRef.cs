using UnityEngine;

namespace Lotus.Roles2.GUI;

public class GuiRef
{
    private ActionButton button;
    protected virtual Sprite Sprite { get; private set; }

    public GuiRef(ActionButton button, Sprite sprite)
    {
        this.button = button;
        this.Sprite = button.graphic.sprite = sprite;
    }

    public GuiRef(ActionButton button)
    {
        this.button = button;
        this.Sprite = button != null ? button.graphic.sprite : null!;
    }

    public Sprite GetSprite() => Sprite;
    public void SetSprite(Sprite sprite)
    {
        button.graphic.sprite = this.Sprite = sprite;
    }
}