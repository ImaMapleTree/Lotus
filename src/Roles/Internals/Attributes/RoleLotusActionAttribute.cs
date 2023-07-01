extern alias JBAnnotations;
using System;
using System.Collections.Generic;
using JBAnnotations::JetBrains.Annotations;
using Lotus.API;
using Lotus.API.Reactive.Actions;
using Lotus.Roles.Internals.Enums;

// ReSharper disable InvalidXmlDocComment

namespace Lotus.Roles.Internals.Attributes;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)] // Inherited = false because inheritance is managed through Subclassing, DO NOT WORRY!
public class RoleActionAttribute: LotusActionAttribute
{
    public bool WorksAfterDeath { get; }
    public bool Blockable { set; get; }
    /// <summary>
    /// If provided, overrides any methods of the same action with the same name from any parent classes
    /// </summary>
    public String? Override;

    public RoleActionAttribute(LotusActionType actionType, bool triggerAfterDeath = false, bool blockable = true, Priority priority = Priority.Normal): base(actionType, priority)
    {
        this.WorksAfterDeath = triggerAfterDeath || actionType is LotusActionType.MyDeath or LotusActionType.SelfExiled;
        this.Blockable = blockable && actionType is not LotusActionType.MyVote;
    }

    public override string ToString() => $"RoleAction(type={ActionType}, Priority={Priority}, Blockable={Blockable}, Subclassing={Subclassing}, Override={Override})";
}

public static class RoleActionTypeMethods
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public static readonly HashSet<LotusActionType> PlayerActions = new();

    public static bool IsPlayerAction(this LotusActionType actionType)
    {
        return actionType switch
        {
            LotusActionType.None => false,
            LotusActionType.AnyPlayerAction => false,
            LotusActionType.OnPet => true,
            LotusActionType.MyEnterVent => true,
            LotusActionType.AnyEnterVent => true,
            LotusActionType.VentExit => true,
            LotusActionType.SuccessfulAngelProtect => false,
            LotusActionType.SabotageStarted => true,
            LotusActionType.SabotagePartialFix => true,
            LotusActionType.SabotageFixed => true,
            LotusActionType.Shapeshift => true,
            LotusActionType.Unshapeshift => true,
            LotusActionType.Attack => true,
            LotusActionType.MyDeath => false,
            LotusActionType.SelfExiled => false,
            LotusActionType.AnyExiled => false,
            LotusActionType.RoundStart => false,
            LotusActionType.RoundEnd => false,
            LotusActionType.SelfReportBody => true,
            LotusActionType.AnyReportedBody => false,
            LotusActionType.TaskComplete => false,
            LotusActionType.FixedUpdate => false,
            LotusActionType.AnyDeath => false,
            LotusActionType.MyVote => true,
            LotusActionType.AnyVote => false,
            LotusActionType.Interaction => false,
            LotusActionType.AnyInteraction => false,
            LotusActionType.OnHoldPet => false,
            LotusActionType.OnPetRelease => false,
            LotusActionType.AnyShapeshift => false,
            LotusActionType.AnyUnshapeshift => false,
            LotusActionType.Chat => false,
            LotusActionType.Disconnect => false,
            LotusActionType.VotingComplete => false,
            LotusActionType.MeetingCalled => true,
            _ => PlayerActions.Contains(actionType)
        };
    }
}