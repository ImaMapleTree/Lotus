using System;
using System.IO;
using TOHTOR.Managers.Friends;
using TOHTOR.Managers.Templates;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;

namespace TOHTOR.Managers;

// TODO: Create copy of local storage in cache folder and have file checking if cached, if file DNE then copy files from cache into main game
[LoadStatic]
public static class PluginDataManager
{
    private const string ModifiableDataDirectoryPath = "./TOHTOR_DATA";
    private static readonly string HiddenDataDirectoryPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "/TownOfHostTheOtherRoles");
    
    private const string TemplateFile = "Templates.txt";
    private const string WordListFile = "BannedWords.txt";
    private const string FriendListFile = "Friends.txt";
    private const string LastKnownAsFile = "LastKnownAs.json";
    private const string TemplateCommandFile = "TemplateCommands.txt";

    public static readonly DirectoryInfo ModifiableDataDirectory;
    public static readonly DirectoryInfo HiddenDataDirectory;

    public static TemplateCommandManager TemplateCommandManager;
    public static TemplateManager TemplateManager;
    public static ChatManager ChatManager;
    public static LastKnownAs LastKnownAs;
    public static FriendManager FriendManager;

    static PluginDataManager()
    {
        ModifiableDataDirectory = new DirectoryInfo(ModifiableDataDirectoryPath);
        HiddenDataDirectory = new DirectoryInfo(HiddenDataDirectoryPath);
        if (!ModifiableDataDirectory.Exists) ModifiableDataDirectory.Create();
        if (!HiddenDataDirectory.Exists) HiddenDataDirectory.Create();
        
        
        TemplateManager = new TemplateManager(ModifiableDataDirectory.GetFile(TemplateFile));
        ChatManager = new ChatManager(ModifiableDataDirectory.GetFile(WordListFile));
        FriendManager = new FriendManager(ModifiableDataDirectory.GetFile(FriendListFile));
        TemplateCommandManager = new TemplateCommandManager(ModifiableDataDirectory.GetFile(TemplateCommandFile));

        LastKnownAs = new LastKnownAs(HiddenDataDirectory.GetFile(LastKnownAsFile));
    }
}