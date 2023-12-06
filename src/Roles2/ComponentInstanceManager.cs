using System;
using VentLib.Utilities.Collections;

namespace Lotus.Roles2;

public class ComponentInstanceManager
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(ComponentInstanceManager));

    private readonly OrderedSet<Type> componentTypes;

    public ComponentInstanceManager(OrderedSet<Type> componentTypes)
    {
        this.componentTypes = componentTypes;
    }

    public GeneratingCIM Generate(UnifiedRoleDefinition definition)
    {
        return new GeneratingCIM(definition, componentTypes);
    }
}