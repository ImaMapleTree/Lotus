extern alias JBAnnotations;
using System;
using System.Collections.Generic;
using JBAnnotations::JetBrains.Annotations;
using Lotus.API;
using Lotus.Roles.Internals.Enums;

// ReSharper disable InvalidXmlDocComment

namespace Lotus.Roles.Internals.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)] // Inherited = false because inheritance is managed through Subclassing, DO NOT WORRY!
public class RoleActionAttribute: Attribute
{
    public RoleActionType ActionType { get; }
    public bool WorksAfterDeath { get; }
    public Priority Priority { get; }
    public bool Blockable { set; get; }
    /// <summary>
    /// If provided, overrides any methods of the same action with the same name from any parent classes
    /// </summary>
    public String? Override;
    /// <summary>
    /// Dictates whether this action should be utilized in subclasses of the class declaring this method <b>Default: True</b>
    /// </summary>
    public bool Subclassing = true;

    public RoleActionAttribute(RoleActionType actionType, bool triggerAfterDeath = false, bool blockable = true, Priority priority = Priority.Normal)
    {
        this.ActionType = actionType;
        this.WorksAfterDeath = triggerAfterDeath || actionType is RoleActionType.MyDeath or RoleActionType.SelfExiled;
        this.Priority = priority;
        this.Blockable = blockable && actionType is not RoleActionType.MyVote;
    }

    public override string ToString() => $"RoleAction(type={ActionType}, Priority={Priority}, Blockable={Blockable}, Subclassing={Subclassing}, Override={Override})";
}

public static class RoleActionTypeMethods
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public static readonly HashSet<RoleActionType> PlayerActions = new();

    public static bool IsPlayerAction(this RoleActionType actionType)
    {
        return actionType switch
        {
            RoleActionType.None => false,
            RoleActionType.AnyPlayerAction => false,
            RoleActionType.OnPet => true,
            RoleActionType.MyEnterVent => true,
            RoleActionType.AnyEnterVent => true,
            RoleActionType.VentExit => true,
            RoleActionType.SuccessfulAngelProtect => false,
            RoleActionType.SabotageStarted => true,
            RoleActionType.SabotagePartialFix => true,
            RoleActionType.SabotageFixed => true,
            RoleActionType.Shapeshift => true,
            RoleActionType.Unshapeshift => true,
            RoleActionType.Attack => true,
            RoleActionType.MyDeath => false,
            RoleActionType.SelfExiled => false,
            RoleActionType.AnyExiled => false,
            RoleActionType.RoundStart => false,
            RoleActionType.RoundEnd => false,
            RoleActionType.SelfReportBody => true,
            RoleActionType.AnyReportedBody => false,
            RoleActionType.TaskComplete => false,
            RoleActionType.FixedUpdate => false,
            RoleActionType.AnyDeath => false,
            RoleActionType.MyVote => true,
            RoleActionType.AnyVote => false,
            RoleActionType.Interaction => false,
            RoleActionType.AnyInteraction => false,
            RoleActionType.OnHoldPet => false,
            RoleActionType.OnPetRelease => false,
            RoleActionType.AnyShapeshift => false,
            RoleActionType.AnyUnshapeshift => false,
            RoleActionType.Chat => false,
            RoleActionType.Disconnect => false,
            RoleActionType.VotingComplete => false,
            RoleActionType.MeetingCalled => true,
            _ => PlayerActions.Contains(actionType)
        };
    }
}