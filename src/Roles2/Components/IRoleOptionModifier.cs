using Lotus.Roles2.Interfaces;
using VentLib.Options.Game;

namespace Lotus.Roles2.Components;

public interface IRoleOptionModifier: IRoleComponent
{
    public GameOptionBuilder ModifyBefore(GameOptionBuilder source) => source;

    public GameOptionBuilder ModifyAfter(GameOptionBuilder source) => source;
}