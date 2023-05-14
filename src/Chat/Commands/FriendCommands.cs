using System.Linq;
using Lotus.Managers;
using Lotus.Utilities;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Chat.Commands;

[Command(CommandFlag.HostOnly, "friends", "friend", "f")]
public class FriendCommands: CommandTranslations, ICommandReceiver
{
    [Command("add", "a")]
    public static void AddFriend(PlayerControl source, CommandContext context)
    {
        void AFriend(PlayerControl p)
        {
            if (p.FriendCode == null)
            {
                Utils.SendMessage(FriendsCommandTranslations.NoFriendcodeText.Formatted(p.name), source.PlayerId);
                return;
            }
            PluginDataManager.FriendManager.AddFriend(p.FriendCode);
            Utils.SendMessage(FriendsCommandTranslations.SuccessText.Formatted(p.name, p.FriendCode), source.PlayerId);
        }
        
        if (context.Args.Length == 0)
        {
            Utils.SendMessage(InvalidUsage, source.PlayerId, ModConstants.Palette.InvalidUsage.Colorize(InvalidUsage));
            return;
        }

        if (int.TryParse(context.Args[0], out int value))
            Utils.PlayerById(value).Handle(AFriend, () => Utils.SendMessage(PlayerNotFoundText.Formatted(""), source.PlayerId));
        else
            PlayerControl.AllPlayerControls.ToArray().FirstOrOptional(p => p.name == context.Join())
                .Handle(AFriend, () => Utils.SendMessage(PlayerNotFoundText.Formatted(context.Join()), source.PlayerId));
    }

    [Command("remove", "r")]
    public static void RemoveFriend(PlayerControl source, int index)
    {
        string friend = PluginDataManager.FriendManager.RemoveFriend(index - 1);
        Utils.SendMessage(FriendsCommandTranslations.RemoveText.Formatted(friend), source.PlayerId);
    }
    
    [Command("list", "l")]
    public static void ListFriends(PlayerControl source)
    {
        string friends = PluginDataManager.FriendManager.Friends()
            .Select(f => (f, PluginDataManager.LastKnownAs.Name(f)))
            .Select((t2, i) => $"{i + 1}. {t2.f}{LastKnownAsString(t2.Item2)}")
            .Fuse("\n");
        
        Utils.SendMessage(friends, source.PlayerId, leftAlign: true);
    }

    private static string LastKnownAsString(string? name)
    {
        return name != null ? " (" + FriendsCommandTranslations.LastKnownAsText.Formatted(name) + ")" : "";
    }

    [Localized("Friends")]
    private static class FriendsCommandTranslations
    {
        [Localized(nameof(NoFriendcodeText))]
        public static string NoFriendcodeText = "Could not add {0}. They do not have a valid friendcode.";

        [Localized(nameof(SuccessText))]
        public static string SuccessText = "Successfully added {0} ({1})";

        [Localized(nameof(RemoveText))]
        public static string RemoveText = "Successfully removed friend {0} from friends list";

        [Localized(nameof(LastKnownAsText))]
        public static string LastKnownAsText = "Known As: {0}";
    }

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (context.Args.Length == 0) BasicCommands.PlayerIds(source, context);
    }
}