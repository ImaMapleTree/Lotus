using System.Collections.Generic;
using System.Linq;

namespace Lotus.Roles2.Interfaces;

public interface IComponentAware<T>: IComponentAware where T: IRoleComponent
{
    public void ReceiveTargetComponents(List<T> components);

    void IComponentAware.ReceiveComponents(List<IRoleComponent> components)
    {
        ReceiveTargetComponents(components.OfType<T>().ToList());
    }
}

public interface IComponentAware
{
    public void ReceiveComponents(List<IRoleComponent> components);
}