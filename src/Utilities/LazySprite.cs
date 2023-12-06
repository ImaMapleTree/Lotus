using System;
using UnityEngine;
using VentLib.Utilities.Optionals;

namespace Lotus.Utilities;

public class LazySprite
{
    private Func<Sprite> spriteLoadingFunction;
    private UnityOptional<Sprite> initializedSprite = UnityOptional<Sprite>.Null();

    public LazySprite(Func<Sprite> spriteLoadingFunction)
    {
        this.spriteLoadingFunction = spriteLoadingFunction;
    }

    public bool Apply(ref Sprite sprite)
    {
        if (sprite != null) return false;
        sprite = spriteLoadingFunction();
        return true;
    }

    public Sprite Get() => initializedSprite.OrElseSet(spriteLoadingFunction);
}