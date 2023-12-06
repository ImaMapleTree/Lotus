using Lotus.Utilities;
using UnityEngine;

namespace Lotus.Roles2.GUI;

public class LazyGuiRef: GuiRef
{
    private LazySprite lazySprite;
    protected override Sprite Sprite => lazySprite.Get();

    public LazyGuiRef(ActionButton button, LazySprite sprite) : base(button, sprite.Get())
    {
        this.lazySprite = sprite;
    }
}