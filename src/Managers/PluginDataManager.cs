using System;
using System.IO;
using Lotus.Managers.Friends;
using Lotus.Managers.Templates;
using Lotus.Managers.Titles;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers;

// TODO: Create copy of local storage in cache folder and have file checking if cached, if file DNE then copy files from cache into main game
[LoadStatic]
public static class PluginDataManager
{
    private const string LegacyDataDirectoryTOHPath = "./TOR_DATA";
    private const string ModifiableDataDirectoryPath = "./LOTUS_DATA";
    private const string ModifiableDataDirectoryPathOld = "./TOHTOR_DATA";
    private static readonly string HiddenDataDirectoryPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "/TownOfHostTheOtherRoles");

    private const string TitleDirectory = "Titles";

    private const string TOHTemplateFile = "template.txt";
    private const string TemplateFile = "Templates.txt";
    private const string TemplateFile2 = "Templates.yaml";

    private const string WordListFile = "BannedWords.txt";
    private const string FriendListFile = "Friends.txt";
    private const string LastKnownAsFile = "LastKnownAs.json";
    private const string TemplateCommandFile = "TemplateCommands.txt";

    public static readonly DirectoryInfo LegacyDataDirectoryTOH;
    public static readonly DirectoryInfo ModifiableDataDirectory;
    public static readonly DirectoryInfo HiddenDataDirectory;

    public static TemplateCommandMigrator TemplateCommandMigrator;
    private static TemplateMigration _templateMigration;
    public static TemplateManager TemplateManager;

    public static ChatManager ChatManager;
    public static LastKnownAs LastKnownAs;
    public static FriendManager FriendManager;

    public static TitleManager TitleManager;


    static PluginDataManager()
    {
        MigrateOldDirectory();

        ModifiableDataDirectory = new DirectoryInfo(ModifiableDataDirectoryPath);
        HiddenDataDirectory = new DirectoryInfo(HiddenDataDirectoryPath);
        LegacyDataDirectoryTOH = new DirectoryInfo(LegacyDataDirectoryTOHPath);
        if (!ModifiableDataDirectory.Exists) ModifiableDataDirectory.Create();
        if (!HiddenDataDirectory.Exists) HiddenDataDirectory.Create();


        TemplateManager = new TemplateManager(ModifiableDataDirectory.GetFile(TemplateFile2));
        _templateMigration = new TemplateMigration(ModifiableDataDirectory.GetFile(TemplateFile), LegacyDataDirectoryTOH.GetFile(TOHTemplateFile));
        ChatManager = new ChatManager(ModifiableDataDirectory.GetFile(WordListFile));
        FriendManager = new FriendManager(ModifiableDataDirectory.GetFile(FriendListFile));
        TemplateCommandMigrator = new TemplateCommandMigrator(ModifiableDataDirectory.GetFile(TemplateCommandFile));
        TitleManager = new TitleManager(ModifiableDataDirectory.GetDirectory(TitleDirectory));

        LastKnownAs = new LastKnownAs(HiddenDataDirectory.GetFile(LastKnownAsFile));

    }

    private static void MigrateOldDirectory()
    {
        DirectoryInfo oldDirectory = new(ModifiableDataDirectoryPathOld);
        if (!oldDirectory.Exists) return;
        try
        {
            oldDirectory.MoveTo(ModifiableDataDirectoryPath);
        }
        catch
        {
            // ignored
        }
    }
}