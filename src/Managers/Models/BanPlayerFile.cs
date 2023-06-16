using System.Collections.Generic;

namespace Lotus.Managers.Models;

public class BanPlayerFile
{
    public List<BannedPlayer> Players { get; set; } = new();
}