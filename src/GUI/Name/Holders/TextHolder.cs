using Lotus.GUI.Name.Components;

namespace Lotus.GUI.Name.Holders;

public class TextHolder : ComponentHolder<TextComponent>
{
    public TextHolder(int line = 0) : base(line)
    {
        Spacing = 1;
    }
}