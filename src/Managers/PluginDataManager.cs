using System;
using System.IO;
using TOHTOR.Managers.Templates;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers;

// TODO: Create copy of local storage in cache folder and have file checking if cached, if file DNE then copy files from cache into main game
[LoadStatic]
public static class PluginDataManager
{
    public const string ModifiableDataDirectoryPath = "./TOHTOR_DATA";
    public static string HiddenDataDirectoryPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "/TownOfHostTheOtherRoles");
    public const string TemplateFile = "Templates.json";
    public const string WordListFile = "BannedWords.txt";

    public static readonly DirectoryInfo ModifiableDataDirectory;
    public static readonly DirectoryInfo HiddenDataDirectory;
    public static TemplateManager TemplateManager;
    public static ChatManager ChatManager;

    static PluginDataManager()
    {
        ModifiableDataDirectory = new DirectoryInfo(ModifiableDataDirectoryPath);
        HiddenDataDirectory = new DirectoryInfo(HiddenDataDirectoryPath);
        if (!ModifiableDataDirectory.Exists) ModifiableDataDirectory.Create();
        if (!HiddenDataDirectory.Exists) HiddenDataDirectory.Create();
        TemplateManager = new TemplateManager(ModifiableDataDirectory.GetFile(TemplateFile));
        ChatManager = new ChatManager(ModifiableDataDirectory.GetFile(WordListFile));
    }
}