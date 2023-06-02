using Lotus.API;
using Lotus.Extensions;
using Lotus.Roles.Interfaces;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace Lotus.Roles.Subroles;

public class Workhorse: Subrole
{
    private int additionalShortTasks;
    private int additionalLongTasks;
    
    public override string Identifier() => "<size=2.2>Θ</size>";

    protected override void PostSetup()
    {
        Tasks.AssignAdditionalTasks(MyPlayer.GetCustomRole(), additionalShortTasks, additionalLongTasks, TaskAssignmentMode.Add);
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        return player.GetCustomRole() is ITaskHolderRole && base.IsAssignableTo(player);
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) => 
        base.Modify(roleModifier).RoleColor(new Color(0.12f, 0.49f, 0.15f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => 
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Additional Short Tasks", Translations.Options.AdditionalShortTasks)
                .AddIntRange(0, 60, 1, 1)
                .BindInt(i => additionalShortTasks = i)
                .Build())
            .SubOption(sub => sub.KeyName("Additional Long Tasks", Translations.Options.AdditionalLongTasks)
                .AddIntRange(0, 60, 1, 1)
                .BindInt(i => additionalLongTasks = i)
                .Build());

    [Localized(nameof(Workhorse))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        internal static class Options
        {
            [Localized(nameof(AdditionalShortTasks))]
            public static string AdditionalShortTasks = "Additional Short Tasks";
            
            [Localized(nameof(AdditionalLongTasks))]
            public static string AdditionalLongTasks = "Additional Long Tasks";
        }
    }
}