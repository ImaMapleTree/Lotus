using Lotus.Utilities;

namespace Lotus.Roles2.Attributes.Options;

public class AttributeContext
{
    public RoleLocalizer Localizer { get; }
    public RoleDefinition Definition { get; }
    public OptionAttributeRepresentation Representation { get; }
    public Reflector Reflector { get; }

    public AttributeContext(RoleLocalizer localizer, RoleDefinition definition, OptionAttributeRepresentation representation, Reflector reflector)
    {
        Localizer = localizer;
        Definition = definition;
        Representation = representation;
        Reflector = reflector;
    }
}