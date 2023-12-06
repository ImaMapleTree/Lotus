extern alias JBAnnotations;
using System;
using Lotus.Roles.Internals.Enums;

namespace Lotus.API.Reactive.Actions;

[JBAnnotations::JetBrains.Annotations.MeansImplicitUseAttribute]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class LotusActionAttribute: Attribute
{
    public LotusActionType ActionType { get; }
    public Priority Priority { get; }

    /// <summary>
    /// Dictates whether this action should be utilized in subclasses of the class declaring this method <b>Default: True</b>
    /// </summary>
    public bool Subclassing = true;

    public LotusActionAttribute(LotusActionType actionType, Priority priority = Priority.Normal)
    {
        ActionType = actionType;
        Priority = priority;
    }

    public override string ToString() => $"GameModeAction(type={ActionType}, Priority={Priority})";
}