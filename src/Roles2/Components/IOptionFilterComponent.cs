using Lotus.Roles2.Attributes.Options;
using Lotus.Roles2.Interfaces;
using VentLib.Options.Game;

namespace Lotus.Roles2.Components;

public interface IOptionFilterComponent: IRoleComponent
{
    public bool PrefilterOption(ref OptionAttributeRepresentation representation) => true;

    public bool IntrafilterOption(OptionAttributeRepresentation representation, ref GameOptionBuilder builder) => true;

    public bool PostfilterOption(OptionAttributeRepresentation representation, ref GameOption option) => true;

}