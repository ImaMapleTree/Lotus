using System.Linq;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.Extensions;
using TOHTOR.Roles;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace TOHTOR.Chat.Commands;

[Command(CommandFlag.InGameOnly, "m", "myrole")]
public class RoleInfoCommand: ICommandReceiver
{
    private int previousLevel = 0;

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (Game.State is GameState.InLobby) return;
        if (context.Args.Length == 0) {
            ShowRoleDescription(source);
            return;
        }
        string pageString = context.Args[0];
        if (!int.TryParse(pageString, out int page) || page <= 1) ShowRoleDescription(source);
        else ShowRoleOptions(source);
    }

    private void ShowRoleDescription(PlayerControl source)
    {
        CustomRole role = source.GetCustomRole();
        string output = $"{role} {role.Faction}:";
        output += $"\n{role.Description}";
        Utils.SendMessage(output, source.PlayerId, leftAlign: true);
    }

    private void ShowRoleOptions(PlayerControl source)
    {
        CustomRole role = source.GetCustomRole();
        string output = $"{role} {role.Faction}:";

        Option? optionMatch = OptionManager.GetManager(file: "role_options.txt").GetOptions().FirstOrDefault(h => h.Name().RemoveHtmlTags() == role.RoleName);
        if (optionMatch == null) { ShowRoleDescription(source); return; }

        foreach (var child in optionMatch.Children) UpdateOutput(ref output, child);

        Utils.SendMessage(output, source.PlayerId, leftAlign: true);
    }

    private void UpdateOutput(ref string output, Option options)
    {
        if (options is not GameOption gameOption) return;
        if (gameOption.Level < previousLevel)
            output += "\n";
        previousLevel = gameOption.Level;
        string valueText = gameOption.Color == Color.white ? gameOption.GetValueText() : gameOption.Color.Colorize(gameOption.GetValueText());
        output += $"\n{gameOption.Name()} => {valueText}";

    }
}