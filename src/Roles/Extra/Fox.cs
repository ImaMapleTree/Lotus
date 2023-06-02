using Lotus.Gamemodes;
using VentLib.Options.Game;

namespace Lotus.Roles.Extra;

public class Fox : NotImplemented
{

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(TestHnsGamemode.HnsTab);
}