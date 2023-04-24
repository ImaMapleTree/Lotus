using System.Linq;
using HarmonyLib;
using TOHTOR.Managers;
using TOHTOR.Managers.Templates;
using TOHTOR.Utilities;
using UnityEngine;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Utilities;

namespace TOHTOR.Chat.Commands;

[Command(new[] { "template", "t", "templates" }, user: CommandUser.Host)]
public class TemplateCommands: ICommandReceiver
{
    [Command("list", "l")]
    private void ListTemplates(PlayerControl source)
    {
        string templates = TemplateManager.GetAllTemplates().Select((t, i) =>
        {
            Color color = t.Profiles.Contains(TemplateManager.GetProfile()) ? Color.green : Color.gray;
            return color.Colorize($"{i}) ") + t.Text;
        }).Join(delimiter: "\n");
        Utils.SendMessage(templates, source.PlayerId, "Templates");
    }

    [Command("create", "c")]
    private void CreateTemplate(PlayerControl source, CommandContext ctx)
    {
        string text = ctx.Args.Join(delimiter: " ");
        TemplateManager.CreateTemplate(text);
        Utils.SendMessage("Successfully created template", source.PlayerId, "Templates");
    }

    [Command("delete", "d")]
    private void DeleteTemplate(PlayerControl source, CommandContext _, int index)
    {
        TemplateManager.DeleteTemplate(index);
        Utils.SendMessage("Successfully deleted template", source.PlayerId, "Templates");
    }

    [Command("preview", "p")]
    private void PreviewTemplate(PlayerControl source, CommandContext _, int index)
    {
        Utils.SendMessage(TemplateManager.GetTemplate(index).Format(source), source.PlayerId, "Templates");
    }

    [Command("tag", "t")]
    private void TagApplyCommand(PlayerControl source, CommandContext _, int index, string tag)
    {
        TemplateManager.TagTemplate(index, tag);
        Utils.SendMessage($"Successfully applied tag \"{tag}\" to template {index}", source.PlayerId, "Templates");
    }

    [Command("untag", "ut")]
    private void UntagCommand(PlayerControl source, CommandContext _, int index, string tag)
    {
        TemplateManager.UntagTemplate(index, tag);
        Utils.SendMessage($"Successfully removed tag \"{tag}\" from template {index}", source.PlayerId, "Templates");
    }

    [Command("tag-delete", "td")]
    private void TagDeleteCommand(PlayerControl source, CommandContext _, string tag)
    {
        TemplateManager.DeleteTag(tag);
        Utils.SendMessage($"Successfully deleted tag \"{tag}\"");
    }

    [Command("tag-preview", "tp")]
    private void TagPreviewCommand(PlayerControl source, CommandContext _, string tag)
    {
        bool isFormatted = TemplateManager.TryFormat(source, tag, out string formatted);
        Utils.SendMessage(!isFormatted ? $"Unable to apply format. Tag={tag}" : formatted, source.PlayerId, "Templates");
    }

    [Command("reload")]
    private void Reload(PlayerControl source)
    {
        TemplateManager.Reload();
        Utils.SendMessage("Reloaded Template Manager", source.PlayerId);
    }

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) Utils.SendMessage("Incorrect usage", source.PlayerId);
    }

    [Command("profile", "profiles")]
    private class ProfileCommands: ICommandReceiver
    {
        public void Receive(PlayerControl source, CommandContext context)
        {
            if (context.Args.Length != 0) return;
            Utils.SendMessage($"Active Profile: {TemplateManager.GetProfile()}", source.PlayerId, "Templates");
        }

        [Command("set")]
        private void SetProfile(PlayerControl source, CommandContext _, string profile)
        {
            TemplateManager.SetProfile(profile);
            Utils.SendMessage($"Set Profile to {profile}", source.PlayerId, "Templates");
        }

        [Command("list")]
        private void ListProfile(PlayerControl source, CommandContext _, int index)
        {
            Utils.SendMessage($"Profiles for Template {index}:\n{TemplateManager.GetTemplate(index).Profiles.Join()}", source.PlayerId, "Templates");
        }

        private void AddProfile(PlayerControl source, CommandContext _, int index, string profile)
        {
            TemplateManager.GetTemplate(index).Profiles.Add(profile);
            TemplateManager.SaveReload();
            Utils.SendMessage($"Added profile \"{profile}\" to template {index}", source.PlayerId, "Templates");
        }

        private void RemoveProfile(PlayerControl source, CommandContext _, int index, string profile)
        {
            TemplateManager.GetTemplate(index).Profiles.Remove(profile);
            TemplateManager.SaveReload();
            Utils.SendMessage($"Removed profile \"{profile}\" from template {index}", source.PlayerId, "Templates");
        }
    }

    private static TemplateManager TemplateManager => PluginDataManager.TemplateManager;
}