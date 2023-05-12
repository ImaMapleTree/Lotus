namespace TOHTOR.Roles.Internals;

public interface ICloneOnSetup
{
    public object CloneIndiscriminate();
}

public interface ICloneOnSetup<out T>: ICloneOnSetup
{
    object ICloneOnSetup.CloneIndiscriminate() => Clone()!;

    public T Clone();
}