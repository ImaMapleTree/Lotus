using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using VentLib.Localization.Attributes;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

namespace TOHTOR.Chat.Commands;

[Localized("Commands")]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class CommandTranslations
{
    public static Color InvalidColor = new(1f, 0.67f, 0.11f);

    [Localized(nameof(InvalidUsage))] protected static string InvalidUsage = "⚠ Invalid Usage ⚠";
    [Localized(nameof(NotPermittedTitle))] protected static string NotPermittedTitle = "⚠ Not Permitted ⚠";
    [Localized(nameof(NotPermittedText))] protected static string NotPermittedText = "You are not permitted to use this command.";
    [Localized("Say.MessageTitle")] protected static string HostMessage = "Host Message";

    [Localized("HostOptions")]
    protected static class HostOptionTranslations
    {
        [Localized(nameof(RoleInfo))] public static string RoleInfo = "★ Role Info ★";
        [Localized(nameof(RoleCategory))] public static string RoleCategory = "★ {0} Roles";
        [Localized(nameof(CurrentRoles))] public static string CurrentRoles = "Current Roles";
    }
}