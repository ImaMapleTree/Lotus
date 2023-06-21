using System.Collections.Generic;
using Hazel;

namespace Lotus.Network;

public class AdvancedBanManager
{
    private static HashSet<long> _bannedIdentifiers = new();


    public static bool VerifyConnection(Connection connection)
    {
        if (_bannedIdentifiers.Contains(connection.EndPoint.Address.Address)) return false;
        return true;
    }
}