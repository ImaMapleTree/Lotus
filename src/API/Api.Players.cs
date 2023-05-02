using System.Collections.Generic;
using TOHTOR.API.Odyssey;

namespace TOHTOR.API;

public partial class Api
{
    public class Players
    {
        public static IEnumerable<PlayerControl> GetAllPlayers() => Game.GetAllPlayers();


    }


}