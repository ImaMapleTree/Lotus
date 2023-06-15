namespace Lotus.Extensions;

public static class PlayerInfoExtensions
{
    public static string ColoredName(this GameData.PlayerInfo playerInfo)
    {
        return playerInfo == null! ? "Unknown" : playerInfo.ColorName.Trim('(', ')');
    }
}