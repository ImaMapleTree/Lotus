using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TOHTOR.Managers;
using TOHTOR.Managers.Templates;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Utilities.Extensions;
using static TOHTOR.Chat.Commands.TemplateCommandsCommands.TemplateCommandTranslations;

namespace TOHTOR.Chat.Commands;

[Command(CommandFlag.HostOnly, "commands", "cmd")]
public class TemplateCommandsCommands: CommandTranslations
{
    private static Regex _commaRegex = new("( *, *)");

    [Command("create", "c")]
    public static void CreateCommand(PlayerControl source, string tag, string aliases)
    {
        List<string> aliasList = _commaRegex.Split(aliases).Where(r => r is not ("" or " ")).ToList();
        var result = CommandManager.Create(tag, aliasList);
        
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (result)
        {
            case ResultType.TagAlreadyExists:
                ErrorHandler(source).Message(TagAlreadyExistsError, tag).Send();
                break;
            case ResultType.Success:
                SuccessHandler(source).Message(CreatedCommandSuccess, tag, aliasList.Fuse()).Send();
                break;
        }
    }

    [Command("remove", "r")]
    public static void RemoveCommand(PlayerControl source, string tag)
    {
        var result = CommandManager.Delete(tag);

        switch (result)
        {
            case ResultType.TagDoesNotExist:
                ErrorHandler(source).Message(TagDoesNotExistError, tag).Send();
                break;
            case ResultType.Success:
                SuccessHandler(source).Message(RemovedCommandSuccess, tag).Send();
                break;
        }
    }

    [Command("alias", "a")]
    public static void AddAlias(PlayerControl source, string tag, string alias)
    {
        var result = CommandManager.AddAlias(tag, alias);

        switch (result)
        {
            case ResultType.AliasAlreadyExists:
                ErrorHandler(source).Message(AliasAlreadyExistsError, alias).Send();
                break;
            case ResultType.TagDoesNotExist:
                ErrorHandler(source).Message(TagDoesNotExistError, tag).Send();
                break;
            case ResultType.Success:
                SuccessHandler(source).Message(AddAliasSuccess, alias, tag).Send();
                break;
        }
    }

    [Command("unalias", "ua")]
    public static void RemoveAlias(PlayerControl source, string tag, string alias)
    {
        var result = CommandManager.RemoveAlias(tag, alias);

        switch (result)
        {
            case ResultType.TagDoesNotExist:
                ErrorHandler(source).Message(TagDoesNotExistError, tag).Send();
                break;
            case ResultType.AliasDoesNotExist:
                ErrorHandler(source).Message(AliasDoesNotExistError, alias).Send();
                break;
            case ResultType.Success:
                SuccessHandler(source).Message(RemoveAliasSuccess, alias, tag).Send();
                break;
        }
    }

    [Command("list", "l")]
    public static void ListAll(PlayerControl source)
    {
        string result = CommandManager.ListAll().Select(item => $"<b>{item.tags}</b>: [{item.aliases.Fuse()}]").Fuse("\n");
        SuccessHandler(source).Message(result).Send();
    }

    private static ChatHandler SuccessHandler(PlayerControl source) => new ChatHandler().Title(t => t.Text(TemplateCommandTitle).Color(new Color(1f, 0.77f, 0.17f)).Build()).Player(source).LeftAlign();
    private static ChatHandler ErrorHandler(PlayerControl source) => new ChatHandler().Title(t => t.Text(CommandError).Color(ModConstants.Palette.KillingColor).Build()).Player(source).LeftAlign();
    
    private static TemplateCommandManager CommandManager => PluginDataManager.TemplateCommandManager;

    [Localized("TemplateCommands")]
    internal static class TemplateCommandTranslations
    {
        [Localized(nameof(TemplateCommandTitle))]
        public static string TemplateCommandTitle = "Template Commands";

        public static string CreatedCommandSuccess = "Successfully created command for tag \"{0}\" with alias(es): {1}";

        public static string RemovedCommandSuccess = "Successfully removed command for tag \"{0}\".";

        public static string AddAliasSuccess = "Successfully added alias \"{0}\" to tag-command \"{1}\".";

        public static string RemoveAliasSuccess = "Successfully removed alias \"{0}\" from tag-command \"{1}\".";
        
        public static string TagAlreadyExistsError = "Could not create command. A command for the tag \"{0}\" already exits.";
        
        public static string TagDoesNotExistError = "Tag \"{0}\" does not exist!";

        public static string AliasDoesNotExistError = "Alias \"{0}\" does not exist!";

        public static string AliasAlreadyExistsError = "Alias \"{0}\" already exists!";
    }
}