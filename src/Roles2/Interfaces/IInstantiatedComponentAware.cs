using System.Collections.Generic;
using System.Linq;

namespace Lotus.Roles2.Interfaces;

public interface IInstantiatedComponentAware<T>: IInstantiatedComponentAware where T: IRoleComponent
{
    public void ReceiveTargetInstantiatedComponents(List<T> components);

    void IInstantiatedComponentAware.ReceiveInstantiatedComponents(List<IRoleComponent> components)
    {
        ReceiveTargetInstantiatedComponents(components.OfType<T>().ToList());
    }
}

public interface IInstantiatedComponentAware: IRoleComponent
{
    public void ReceiveInstantiatedComponents(List<IRoleComponent> components);
}