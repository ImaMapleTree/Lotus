using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TOHTOR.Factions.Neutrals;
using TOHTOR.Managers;
using TOHTOR.Options;
using TOHTOR.Roles;
using TOHTOR.Roles.Internals;
using TOHTOR.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Options.IO;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Chat.Commands;

[Command("n", "now")]
public class OptionInfoCommands: ICommandReceiver
{
    private static Regex descriptionRegex = new("^.*#.*$", RegexOptions.Multiline);

    [Command("r", "role", "roles")]
    public void ListRoleOptions(PlayerControl source)
    {
        string? factionName = null;
        string text = "Current Roles:\n";

        OrderedDictionary<string, List<CustomRole>> rolesByFaction = new();

        string FactionName(CustomRole role)
        {
            if (role.Faction is not Solo) return role.Faction.Name();
            return role.SpecialType is SpecialType.NeutralKilling ? "Neutral Killers" : "Neutral";
        }

        CustomRoleManager.AllRoles.ForEach(r => rolesByFaction.GetOrCompute(FactionName(r), () => new List<CustomRole>()).Add(r));

        rolesByFaction.GetValues().SelectMany(s => s).ForEach(r =>
        {

            if (r.Count == 0 || r.Chance == 0) return;

            string fName = FactionName(r);
            if (factionName != fName)
            {
                text += $"\n★ {fName} Roles\n";
                factionName = fName;
            }


            text += $"{r.RoleName}: {r.Count} × {r.Chance}%";
            if (r.Count > 1) text += $" (+ {r.AdditionalChance}%)\n";
            else text += "\n";

            string optionText = descriptionRegex.Replace(OptionWriter.WriteAsString(r.Options), "").Split("\n")[4..].Fuse("\n");
            text += optionText.Replace("*", "•") + "\n";
        });

        Utils.SendMessage(text, source.PlayerId, "◯ Role Info ◯", true);
    }

    public void ListNormalOptions(PlayerControl source)
    {
        string text = GeneralOptions.AllOptions
            .Select(o => o.IsTitle ? $"★ {o.Name()} ★\n" : descriptionRegex.Replace(OptionWriter.WriteAsString(o), "").Replace("*", "•"))
            .Fuse("\n");
        Utils.SendMessage(text, source.PlayerId, "◯ General Info ◯", true);
    }


    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) ListNormalOptions(source);
    }
}