using System.Collections.Generic;
using System.IO;
using System.Linq;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace Lotus.Managers.Friends;

public class FriendManager
{
    private OrderedDictionary<string, string> friendDictionary = new();
    private Dictionary<string, string> lastKnownAsDictionary = new();
    private FileInfo file;

    internal FriendManager(FileInfo fileInfo)
    {
        this.file = fileInfo;
        StreamReader reader = new(file.Open(FileMode.OpenOrCreate));
        List<string> lines = reader.ReadToEnd().Split("\n").Where(l => l != "\n").Where(l => l != "").Select(f => f.Replace("\r", "")).Distinct().ToList();
        reader.Close();
        lines.ForEach(l =>
        {
            string[] split = l.Split(" | ");
            if (split.Length < 2) return;
            friendDictionary[split[0]] = split[1];
        });
    }

    public void AddKnownName(string friendcode, string name)
    {
        if (friendDictionary.ContainsKey(friendcode)) friendDictionary[friendcode] = name;
        else lastKnownAsDictionary[friendcode] = name;
    }

    public bool IsFriend(string friendcode)
    {
        return AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame || friendDictionary.ContainsKey(friendcode);
    }

    public bool IsFriend(PlayerControl player)
    {
        if (player == null) return false;
        return AmongUsClient.Instance.NetworkMode is NetworkModes.LocalGame || player.FriendCode != "" && IsFriend(player.FriendCode);
    }

    public void AddFriend(string friendcode)
    {
        if (friendDictionary.ContainsKey(friendcode)) return;
        friendDictionary[friendcode] = lastKnownAsDictionary.GetValueOrDefault(friendcode, "");
        Save();
    }

    public string RemoveFriend(int index)
    {
        string friend = (string)friendDictionary[index]!;
        friendDictionary.RemoveAt(index);
        Save();
        return friend;
    }

    public List<(string, string)> Friends() => friendDictionary.Select(kv => (kv.Key, kv.Value)).ToList();

    public void Save()
    {
        StreamWriter writer = new(file.Open(FileMode.Create));
        friendDictionary.ForEach(kv => writer.Write($"{kv.Key} | {kv.Value}" + "\n"));
        writer.Close();
    }
}