using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.Chat.Patches;
using Lotus.Managers;
using Lotus.Managers.Hotkeys;
using Lotus.Managers.Templates;
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
        SuccessMsg(TemplateCommandTranslations.CreatedTemplateText.Formatted(id + 1)).Send();
    }

    [Command(CommandFlag.HostOnly, "edit", "e")]
    public static void EditTemplate(PlayerControl source, int id, string text)
    {
        if (!Templates.EditTemplate(id - 1, text)) ErrorMsg(TemplateCommandTranslations.ErrorEditingTemplateText.Formatted(id)).Send(source);
        else SuccessMsg(TemplateCommandTranslations.SuccessEditingTemplateText.Formatted(id)).Send(source);
    }
    
    [Command(CommandFlag.HostOnly, "remove", "r")]
    public static void RemoveTemplate(PlayerControl source, int id)
    {
        if (!Templates.DeleteTemplate(id - 1)) ErrorMsg(TemplateCommandTranslations.ErrorRemovingTemplateText).Send(source);
        else SuccessMsg(TemplateCommandTranslations.SuccessRemovingTemplateText.Formatted(id)).Send(source);
    }

    [Command(CommandFlag.HostOnly, "list", "l")]
    public static void ListTemplates(PlayerControl source)
    {
        string templates = Templates.ListTemplates.Select((t, i) => TemplateText(i + 1, t)).Fuse("\n\n");
        SuccessMsg(templates).Send(source);
    }

    [Command(CommandFlag.HostOnly, "tag", "t")]
    public static void TagTemplate(PlayerControl source, int id, string tag)
    {
        if (!Templates.TagTemplate(id - 1, tag)) ErrorMsg(TemplateCommandTranslations.ErrorTaggingTemplateText.Formatted(tag, id)).Send(source);
        else SuccessMsg(TemplateCommandTranslations.SuccessTaggingTemplateText.Formatted(tag, id)).Send(source);
    }

    [Command(CommandFlag.HostOnly, "untag", "ut")]
    public static void UntagTemplate(PlayerControl source, int id)
    {
        if (!Templates.UntagTemplate(id - 1)) ErrorMsg(TemplateCommandTranslations.ErrorRemovingTagText.Formatted(id)).Send(source);
        else SuccessMsg(TemplateCommandTranslations.SuccessRemovingTagText.Formatted(id)).Send(source);
    }

    [Command(CommandFlag.HostOnly, "preview", "p")]
    public static void Preview(PlayerControl source, int id)
    {
        if (!Templates.TryFormat(source, id - 1, out string text)) ErrorMsg(TemplateCommandTranslations.ErrorPreviewingTemplateText.Formatted(id)).Send(source);
        else SuccessMsg(text).Send(source);
    }

    private static void InternalShow(PlayerControl source, CommandContext ctx, bool showToAll = true)
    {
        if (ctx.Args.Length == 0) ChatHandlers.InvalidCmdUsage().Send(source);
        bool success;
        IEnumerable<PlayerControl> players = showToAll ? Game.GetAllPlayers() : new [] {source };
        players.ForEach(p =>
        {
            string text = "";
            if (int.TryParse(ctx.Args[0], out int result)) success = Templates.TryFormat(p, result - 1, out text);
            else success = Templates.TryFormat(p, ctx.Join(), out text, true);
            if (!success)
            {
                ErrorMsg(TemplateCommandTranslations.ErrorShowingTemplateText.Formatted(p.name)).Send(source);
                return;
            }
            SuccessMsg(text).Send(p);
        });
    }

    [Command(CommandFlag.HostOnly, "tags")]
    public static void ListTags(PlayerControl source)
    {
        string tags = Templates.AllTags().Select((t, i) => $"{i + 1}. {t.tag}: {t.description}").Fuse("\n\n");
        SuccessMsg(tags).Send(source);
    }

    [Command(CommandFlag.HostOnly, "variables", "v")]
    public static void ListVariables(PlayerControl source)
    {
        string variables = Template.TemplateVariables.Select(t => $"<b>{t.Key}</b>: {t.Value}").Fuse("\n");
        SuccessMsg(variables).Send(source);
    }

    [Command(CommandFlag.HostOnly, "reload")]
    public static void Reload(PlayerControl source)
    {
        Templates.Reload();
        SuccessMsg(TemplateCommandTranslations.ReloadTemplatesText).Send(source);
    }

    private static string TemplateText(int i, Template t)
    {
        string tagText = t.Tag != null ? $"({t.Tag}) " : "";
        return $"{i}. {tagText}{t.Text}";
    }

    private static ChatHandler SuccessMsg(string message) => ChatHandler.Of(message, title: TemplateTitle).LeftAlign();
    private static ChatHandler ErrorMsg(string message) => ChatHandler.Of(message, title: CommandError).LeftAlign();
    
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

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) return;
        if (source.IsHost()) RpcSendChatPatch.EatCommand = true;
        
        if (source.IsHost() && HotkeyManager.HoldingRightShift) Game.GetDeadPlayers().ForEach(p => InternalShow(p, context));
        else InternalShow(source, context, source.IsHost());
    }
}