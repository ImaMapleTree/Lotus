using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Managers;
using Lotus.Managers.Templates;
using Lotus.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

[Command("template", "t", "templates")]
public class TemplateCommands: CommandTranslations, ICommandReceiver
{
    public static string TemplateTitle = ModConstants.Palette.GeneralColor4.Colorize("Templates");

    [Command(CommandFlag.HostOnly, "create", "c")]
    public static void CreateTemplate(PlayerControl source, string text)
    {
        int id = Templates.CreateTemplate(text);
        Utils.SendMessage(TemplateCommandTranslations.CreatedTemplateText.Formatted(id + 1), source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "edit", "e")]
    public static void EditTemplate(PlayerControl source, int id, string text)
    {
        if (!Templates.EditTemplate(id - 1, text)) Utils.SendMessage(TemplateCommandTranslations.ErrorEditingTemplateText.Formatted(id), source.PlayerId, CommandError, true);
        else Utils.SendMessage(TemplateCommandTranslations.SuccessEditingTemplateText.Formatted(id), source.PlayerId, TemplateTitle, true);
    }
    
    [Command(CommandFlag.HostOnly, "remove", "r")]
    public static void RemoveTemplate(PlayerControl source, int id)
    {
        if (!Templates.DeleteTemplate(id - 1)) Utils.SendMessage(TemplateCommandTranslations.ErrorRemovingTemplateText, source.PlayerId, CommandError, true);
        else Utils.SendMessage(TemplateCommandTranslations.SuccessRemovingTemplateText.Formatted(id), source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "list", "l")]
    public static void ListTemplates(PlayerControl source)
    {
        string templates = Templates.ListTemplates.Select((t, i) => TemplateText(i + 1, t)).Fuse("\n");
        Utils.SendMessage(templates, source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "tag", "t")]
    public static void TagTemplate(PlayerControl source, int id, string tag)
    {
        if (!Templates.TagTemplate(id - 1, tag)) Utils.SendMessage(TemplateCommandTranslations.ErrorTaggingTemplateText.Formatted(tag, id), source.PlayerId, CommandError, true);
        else Utils.SendMessage(TemplateCommandTranslations.SuccessTaggingTemplateText.Formatted(tag, id), source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "untag", "ut")]
    public static void UntagTemplate(PlayerControl source, int id)
    {
        if (!Templates.UntagTemplate(id - 1)) Utils.SendMessage(TemplateCommandTranslations.ErrorRemovingTagText.Formatted(id), source.PlayerId, CommandError, true);
        else Utils.SendMessage(TemplateCommandTranslations.SuccessRemovingTagText.Formatted(id), source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "preview", "p")]
    public static void Preview(PlayerControl source, int id)
    {
        if (!Templates.TryFormat(source, id - 1, out string text)) Utils.SendMessage(TemplateCommandTranslations.ErrorPreviewingTemplateText.Formatted(id), source.PlayerId, CommandError, true);
        else Utils.SendMessage(text, source.PlayerId, leftAlign: true);
    }

    [Command(CommandFlag.HostOnly, "show", "s")]
    public static void Show(PlayerControl source, CommandContext ctx)
    {
        InternalShow(source, ctx);
    }

    private static void InternalShow(PlayerControl source, CommandContext ctx, bool showToAll = true)
    {
        if (ctx.Args.Length == 0) Utils.SendMessage(InvalidUsage, source.PlayerId, InvalidUsage, true);
        bool success;
        IEnumerable<PlayerControl> players = showToAll ? Game.GetAllPlayers() : new [] {source };
        players.ForEach(p =>
        {
            string text = "";
            if (int.TryParse(ctx.Args[0], out int result)) success = Templates.TryFormat(p, result - 1, out text);
            else success = Templates.TryFormat(p, ctx.Join(), out text, true);
            if (!success)
            {
                Utils.SendMessage(TemplateCommandTranslations.ErrorShowingTemplateText.Formatted(p.name), source.PlayerId, CommandError, true);
                return;
            }
            Utils.SendMessage(text, p.PlayerId, leftAlign: true);
        });
    }

    [Command(CommandFlag.HostOnly, "tags")]
    public static void ListTags(PlayerControl source)
    {
        string tags = Templates.AllTags().Select((t, i) => $"{i + 1}. {t.tag}: {t.description}").Fuse("\n");
        Utils.SendMessage(tags, source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "variables", "v")]
    public static void ListVariables(PlayerControl source)
    {
        string variables = Template.TemplateVariables.Select(t => $"<b>{t.Key}</b>: {t.Value}").Fuse("\n");
        Utils.SendMessage(variables, source.PlayerId, TemplateTitle, true);
    }

    [Command(CommandFlag.HostOnly, "reload")]
    public static void Reload(PlayerControl source)
    {
        Templates.Reload();
        Utils.SendMessage(TemplateCommandTranslations.ReloadTemplatesText, source.PlayerId, TemplateTitle, true);
    }

    private static string TemplateText(int i, Template t)
    {
        string tagText = t.Tag != null ? $"({t.Tag}) " : "";
        return $"{i}. {tagText}{t.Text}";
    }
    
    private static TemplateManager Templates => PluginDataManager.TemplateManager;

    [Localized("Template")]
    private static class TemplateCommandTranslations
    {
        [Localized(nameof(CreatedTemplateText))]
        public static string CreatedTemplateText = "Successfully created template (ID: {0}).";

        [Localized(nameof(ErrorRemovingTemplateText))]
        public static string ErrorRemovingTemplateText = "Error removing template.";
        
        [Localized(nameof(SuccessRemovingTemplateText))]
        public static string SuccessRemovingTemplateText = "Successfully removed template (ID: {0}).";

        [Localized(nameof(SuccessTaggingTemplateText))]
        public static string SuccessTaggingTemplateText = "Successfully added tag \"{0}\" to template (ID: {1}).";

        [Localized(nameof(ErrorTaggingTemplateText))]
        public static string ErrorTaggingTemplateText = "Error adding tag \"{0}\" to template (ID: {1}).";

        [Localized(nameof(SuccessRemovingTagText))]
        public static string SuccessRemovingTagText = "Successfully removed tags from template (ID: {0}).";

        [Localized(nameof(ErrorRemovingTagText))]
        public static string ErrorRemovingTagText = "Error removing tags from template (ID: {0}).";

        [Localized(nameof(ErrorPreviewingTemplateText))]
        public static string ErrorPreviewingTemplateText = "Error previewing template (ID: {0}).";

        [Localized(nameof(ErrorShowingTemplateText))]
        public static string ErrorShowingTemplateText = "Error showing template to {0}.";

        [Localized(nameof(SuccessEditingTemplateText))]
        public static string SuccessEditingTemplateText = "Successfully edited template (ID: {0}).";

        [Localized(nameof(ErrorEditingTemplateText))]
        public static string ErrorEditingTemplateText = "Error editing template (ID: {0}).";

        [Localized(nameof(ReloadTemplatesText))]
        public static string ReloadTemplatesText = "Successfully reloaded templates.";
    }

    public bool Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length > 0) InternalShow(source, context, false);
        return false;
    }
}