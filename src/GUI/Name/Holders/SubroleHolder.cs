using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Components;
using Lotus.Options;
using Lotus.Options.LotusImpl;
using Lotus.Patches.Actions;
using LotusTrigger.Options;
using VentLib.Utilities.Extensions;

namespace Lotus.GUI.Name.Holders;

public class SubroleHolder : ComponentHolder<SubroleComponent>
{
    public SubroleHolder(int line = 0) : base(line)
    {
        Spacing = GeneralOptions.GameplayOptions.ModifierTextMode is ModifierTextMode.Off ? 0 : 1;
    }

    public override string Render(PlayerControl player, GameState state)
    {
        if (player.IsShapeshifted()) return "";
        List<string> endString = new();
        ViewMode lastMode = ViewMode.Absolute;

        ModifierTextMode textMode = GeneralOptions.GameplayOptions.ModifierTextMode;

        // TODO: if laggy investigate ways to circumvent ToArray() call here
        foreach ((int i, SubroleComponent component) in this.Where(p => p.GameStates().Contains(state)).ToArray().Where(p => p.Viewers().Any(pp => pp.PlayerId == player.PlayerId)).Indexed())
        {
            ViewMode newMode = component.ViewMode();
            if (newMode is ViewMode.Replace or ViewMode.Absolute || lastMode is ViewMode.Overriden) endString.Clear();
            lastMode = newMode;
            string? text;
            switch (textMode)
            {
                case ModifierTextMode.First when i == 0 && state is not GameState.InMeeting:
                    text = component.GenerateText();
                    break;
                case ModifierTextMode.All when state is not GameState.InMeeting:
                    text = component.GenerateText();
                    break;
                case ModifierTextMode.Off:
                default:
                    text = component.GenerateIdentifier();
                    break;
            }
            if (text == null!) continue;
            endString.Add(text);
            if (newMode is ViewMode.Absolute) break;
        }

        string newString = endString.Count == 0 ? "" :  " " + endString.Join(delimiter: " ".Repeat(Spacing - 1));

        updated[player.PlayerId] = CacheStates.GetValueOrDefault(player.PlayerId, "") != newString;
        return CacheStates[player.PlayerId] = newString;
    }
}