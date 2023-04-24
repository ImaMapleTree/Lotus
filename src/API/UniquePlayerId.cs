using System.Linq;
using HarmonyLib;
using InnerNet;
using TOHTOR.Utilities;
using VentLib.Options.Interfaces;
using VentLib.Options.IO;
using VentLib.Options.Processors;
using VentLib.Utilities.Attributes;

namespace TOHTOR.API;

[LoadStatic]
public class UniquePlayerId
{
    public static UniquePlayerId EmptyId = new() { ID = "", isUnique = false };
    static UniquePlayerId()
    {
        UniquePlayerIdValueProcessor processor = new();
        ValueTypeProcessors.AddTypeProcessor(processor);
    }

    private string? friendCode;
    private string ID { get; init; } = "";
    private bool isUnique;

    public static UniquePlayerId From(byte playerId) => Utils.PlayerById(playerId).Map(From).OrElse(EmptyId);

    public static UniquePlayerId From(PlayerControl player) => new(player.FriendCode);

    public static UniquePlayerId From(ClientData clientData) => new(clientData.FriendCode);

    private UniquePlayerId()
    {
    }

    // We utilize AU's friend code to create the unique ID
    private UniquePlayerId(string? friendcode)
    {
        isUnique = friendcode != null;
        if (friendcode != null) ID = BasicEncryption(friendcode);
        this.friendCode = friendcode;
    }

    public string ToFriendcode() => isUnique ? friendCode ??= BasicDecryption(ID) : "";

    public override bool Equals(object? obj)
    {
        if (!isUnique) return false;
        return obj is UniquePlayerId { isUnique: true } uniquePlayerId && uniquePlayerId.friendCode == friendCode;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public override string ToString() => ID;









    private const int Shift = 15;

    // Very VERY trivial encryption just to slightly prevent tampering with data... overall doesn't matter
    private static string BasicEncryption(string originalString) => originalString.Select(c => (char)(c + Shift)).Reverse().Join(delimiter: "");

    private static string BasicDecryption(string encryptedString) => encryptedString.Select(c => (char)(c - Shift)).Reverse().Join(delimiter: "");

    private class UniquePlayerIdValueProcessor: IValueTypeProcessor<UniquePlayerId>
    {
        public UniquePlayerId Read(MonoLine input) => new() { isUnique = true, ID = input.Content };

        public MonoLine Write(UniquePlayerId value, MonoLine output)
        {
            return output.Of(value.ID);
        }
    }
}