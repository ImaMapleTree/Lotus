using System.Collections.Generic;
using System.Linq;

namespace Lotus.Utilities;

public class FriendUtils
{
    public static HashSet<string> Friends = new();


    public static bool IsFriend(PlayerControl player)
    {
        if (AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame) return true;
        if (player == null || player.FriendCode == null) return false;
        FriendsListManager friendsListManager = FriendsListManager.Instance;
        if (friendsListManager == null) return Friends.Contains(player.FriendCode);

        Friends = friendsListManager.Friends.ToArray().Select(f => f.FriendCode).ToHashSet();
        return Friends.Contains(player.FriendCode);
    }
}