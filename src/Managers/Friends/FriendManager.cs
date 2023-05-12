using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TOHTOR.Managers.Friends;

public class FriendManager
{
    private List<string> friends = new();
    private FileInfo file;

    internal FriendManager(FileInfo fileInfo)
    {
        this.file = fileInfo;
        StreamReader reader = new(file.Open(FileMode.OpenOrCreate));
        friends = reader.ReadToEnd().Split("\n").Where(l => l != "\n").Where(l => l != "").Select(f => f.Replace("\r", "")).Distinct().ToList();
        reader.Close();
    }

    public bool IsFriend(string friendcode)
    {
        return AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame || friends.Contains(friendcode);
    }

    public bool IsFriend(PlayerControl player)
    {
        if (player == null) return false;
        return player.FriendCode != "" && IsFriend(player.FriendCode);
    }

    public void AddFriend(string friendcode)
    {
        if (friends.Contains(friendcode)) return;
        friends.Add(friendcode);
        Save();
    }

    public string RemoveFriend(int index)
    {
        string friend = friends[index];
        friends.RemoveAt(index);
        Save();
        return friend;
    }

    public List<string> Friends() => friends.ToList();

    public void Save()
    {
        StreamWriter writer = new(file.Open(FileMode.Create));
        friends.ForEach(f => writer.Write(f + "\n"));
        writer.Close();
    }
}