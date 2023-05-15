using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Factions;
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Undead;
using Lotus.GUI;
using Lotus.Managers;
using Lotus.Options;
using Lotus.Roles.Extra;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Roles.Subroles;
using Lotus.API;
using Lotus.Extensions;
using Lotus.Logging;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization;
using VentLib.Logging;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Debug.Profiling;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace Lotus.Roles;

// Some people hate using "Base" and "Abstract" in class names but I used both so now I'm a war-criminal :)
public abstract class AbstractBaseRole
{
    public PlayerControl MyPlayer { get; protected set; } = null!;
    private static bool ROLE_DEBUG = true;

    public static T Ref<T>() where T : CustomRole
    {
        int roleId = CustomRoleManager.GetRoleId(typeof(T));
        if (roleId != -1) return (T)CustomRoleManager.GetRoleFromId(roleId);
        if (ROLE_DEBUG)
            VentLogger.Warn($"Illegally Constructing Role for {typeof(T)}", "RoleWarning");
        return (T)typeof(T).GetConstructor(Array.Empty<Type>()).Invoke(null);
    }

    public RoleEditor? Editor { get; internal set; }
    private static List<RoleEditor> _editors = new();


    public string Description => Localizer.Translate($"Roles.{EnglishRoleName.RemoveHtmlTags()}.Description");
    public string Blurb => Localizer.Translate($"Roles.{EnglishRoleName.RemoveHtmlTags()}.Blurb");

    public string RoleName {
        get {
            string name = Localizer.Translate($"Roles.{EnglishRoleName.RemoveHtmlTags()}.RoleName", useCache: false);
            return name == "N/A" ? EnglishRoleName : name;
        }
    }

    public RoleTypes RealRole => DesyncRole ?? VirtualRole;
    public RoleTypes? DesyncRole;
    public RoleTypes VirtualRole;
    public IFaction Faction { get; set; } = FactionInstances.Solo;
    public SpecialType SpecialType = SpecialType.None;
    public Color RoleColor = Color.white;
    public bool IsSubrole { get; private set; }
    public int Chance { get; private set;  }
    public int Count { get; private set; }
    public int AdditionalChance { get; private set; }
    public bool BaseCanVent;

    public RoleFlag RoleFlags;
    public readonly List<CustomRole> LinkedRoles = new();

    internal GameOption Options;
    internal Assembly DeclaringAssembly = Assembly.GetCallingAssembly();

    public string EnglishRoleName { get; private set; }
    private readonly Dictionary<RoleActionType, List<RoleAction>> roleActions = new();

    protected List<GameOptionOverride> roleSpecificGameOptionOverrides = new();

    protected AbstractBaseRole()
    {
        this.EnglishRoleName = this.GetType().Name.Replace("CRole", "").Replace("Role", "");
        VentLogger.Debug($"Role Name: {EnglishRoleName}");
        CreateInstanceBasedVariables();
        // Why? Modify may reference uncreated options, yet when setting up options developers may try to reference
        // RoleColor (which is white until after Modify)
        // To solve this we call Modify to TRY to setup the role color, crashing once it requires uncreated options
        // The modify at the end of this method is the "real" modify
        RoleModifier _;
        try {
            _ = _editors.Aggregate(Modify(new RoleModifier(this)), (current, editor) => editor.HookModifier(current));
        } catch { }
        this.roleSpecificGameOptionOverrides.Clear();

        GameOptionBuilder optionBuilder = _editors.Aggregate(GetGameOptionBuilder(), (current, editor) => editor.HookOptions(current));

        LinkedRoles.ForEach(r =>
        {
            optionBuilder.SubOption(_ => r.GetGameOptionBuilder().IsHeader(false).Build());
            CustomRoleManager.AddRole(r);
        });

        Options = optionBuilder.Build();

        if (!RoleFlags.HasFlag(RoleFlag.DontRegisterOptions) && Options.GetValueText() != "N/A")
        {
            if (!RoleFlags.HasFlag(RoleFlag.Hidden) && Options.Tab == null)
            {
                if (GetType() == typeof(Impostor)) Options.Tab = DefaultTabs.HiddenTab;
                else if (GetType() == typeof(Engineer)) Options.Tab = DefaultTabs.HiddenTab;
                else if (GetType() == typeof(Scientist)) Options.Tab = DefaultTabs.HiddenTab;
                else if (GetType() == typeof(Crewmate)) Options.Tab = DefaultTabs.HiddenTab;
                else if (GetType() == typeof(GuardianAngel)) Options.Tab = DefaultTabs.HiddenTab;
                else
                {

                    if (this is GM)
                    {
                        /*ignored*/
                    }

                    else if (this is Subrole)
                        Options.Tab = DefaultTabs.MiscTab;
                    else if (this.Faction is ImpostorFaction)
                        Options.Tab = DefaultTabs.ImpostorsTab;
                    else if (this.Faction is Crewmates)
                        Options.Tab = DefaultTabs.CrewmateTab;
                    else if (this.Faction is TheUndead)
                        Options.Tab = DefaultTabs.NeutralTab;
                    else if (this.SpecialType is SpecialType.NeutralKilling or SpecialType.Neutral)
                        Options.Tab = DefaultTabs.NeutralTab;
                    else
                        Options.Tab = DefaultTabs.MiscTab;
                }
            }
            Options.Register(CustomRoleManager.RoleOptionManager, OptionLoadMode.LoadOrCreate);
        }

        SetupRoleActions();
        //options.valueHolder?.UpdateBinding();
        _ = _editors.Aggregate(Modify(new RoleModifier(this)), (current, editor) => editor.HookModifier(current));
        //options. = RoleName;
    }

    private void SetupRoleActions()
    {
        Enum.GetValues<RoleActionType>().Do(action => this.roleActions.Add(action, new List<RoleAction>()));
        this.GetType().GetMethods(AccessFlags.InstanceAccessFlags)
            .SelectMany(method => method.GetCustomAttributes<RoleActionAttribute>().Select(a => (a, method)))
            .Where(t => t.a.Subclassing || t.method.DeclaringType == this.GetType())
            .Select(t => new RoleAction(t.Item1, t.method))
            .Do(AddRoleAction);
    }

    private void AddRoleAction(RoleAction action)
    {
        List<RoleAction> currentActions = this.roleActions.GetValueOrDefault(action.ActionType, new List<RoleAction>());

        if (action.Attribute.Override != null) {
            int overrideIndex = currentActions.FindIndex(m => m.method.Name == action.Attribute.Override);
            if (overrideIndex != -1) currentActions[overrideIndex] = action;
            this.roleActions[action.ActionType] = currentActions;
            return;
        }

        VentLogger.Log(LogLevel.All, $"Registering Action {action.ActionType} => {action.method.Name} (from: \"{action.method.DeclaringType}\")", "RegisterAction");
        if (action.ActionType is RoleActionType.FixedUpdate &&
            currentActions.Count > 0)
            throw new ConstraintException("RoleActionType.FixedUpdate is limited to one per class. If you're inheriting a class that uses FixedUpdate you can add Override=METHOD_NAME to your annotation to override its Update method.");

        if (action.Attribute.Subclassing || action.method.DeclaringType == this.GetType())
            currentActions.Add(action);

        this.roleActions[action.ActionType] = currentActions;
    }

    public void  Trigger(RoleActionType actionType, ref ActionHandle handle, params object[] parameters)
    {
        if (!AmongUsClient.Instance.AmHost || Game.State is GameState.InLobby) return;

        uint id = Profilers.Global.Sampler.Start("Action: " + actionType);
        if (actionType == RoleActionType.FixedUpdate)
        {
            List<RoleAction> methods = roleActions[RoleActionType.FixedUpdate];
            if (methods.Count == 0)
            {
                Profilers.Global.Sampler.Discard(id);
                return;
            }
            methods[0].ExecuteFixed(this);
            Profilers.Global.Sampler.Stop(id);
            return;
        }

        handle.ActionType = actionType;
        parameters = parameters.AddToArray(handle);
        // Block ALL triggers if not host

        foreach (var action in roleActions[actionType].Sorted(a => (int)a.Priority))
        {
            if (handle.IsCanceled) continue;
            if (MyPlayer == null || !MyPlayer.IsAlive() && !action.TriggerWhenDead) continue;

            try
            {
                if (actionType.IsPlayerAction())
                {
                    Hooks.PlayerHooks.PlayerActionHook.Propagate(new PlayerActionHookEvent(MyPlayer, action, parameters));
                    Game.TriggerForAll(RoleActionType.AnyPlayerAction, ref handle, MyPlayer, action, parameters);
                }

                if (handle.IsCanceled) continue;

                action.Execute(this, parameters);
            }
            catch (Exception e)
            {
                VentLogger.Exception(e, $"Failed to execute RoleAction {action}.");
            }
        }
        Profilers.Global.Sampler.Stop(id);
    }

    // lol this method is such a hack it's funny
    public IEnumerable<(RoleAction, AbstractBaseRole)> GetActions(RoleActionType actionType) => roleActions[actionType].Select(action => (action, this));

    protected void CreateInstanceBasedVariables()
    {
        this.GetType().GetFields(AccessFlags.InstanceAccessFlags)
            .Where(f => f.GetCustomAttribute<NewOnSetupAttribute>() != null)
            .Select(f => new NewOnSetup(f, f.GetCustomAttribute<NewOnSetupAttribute>()!.UseCloneIfPresent))
            .ForEach(CreateAnnotatedFields);
        this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.FieldType == typeof(Cooldown) || (f.FieldType.IsGenericType && typeof(Optional<>).IsAssignableFrom(f.FieldType.GetGenericTypeDefinition())))
            .Do(f =>
            {
                if (f.FieldType.GetCustomAttribute<NewOnSetupAttribute>() != null) CreateAnnotatedFields(new NewOnSetup(f, f.FieldType.GetCustomAttribute<NewOnSetupAttribute>()!.UseCloneIfPresent));
                else if (f.FieldType == typeof(Cooldown)) CreateCooldown(f);
                else CreateOptional(f);
            });
    }

    private void CreateAnnotatedFields(NewOnSetup setupRules)
    {

        FieldInfo field = setupRules.FieldInfo;
        object? currentValue = field.GetValue(this);
        object newValue;

        if (setupRules.UseCloneIfPresent && currentValue is ICloneOnSetup cos)
        {
            field.SetValue(this, cos.CloneIndiscriminate());
            return;
        }
        
        MethodInfo? cloneMethod = field.FieldType.GetMethod("Clone", AccessFlags.InstanceAccessFlags, Array.Empty<Type>());
        if (currentValue == null || cloneMethod == null || !setupRules.UseCloneIfPresent)
            try {
                newValue = AccessTools.CreateInstance(field.FieldType);
            } catch (Exception e) {
                VentLogger.Exception(e);
                throw new ArgumentException($"Error during \"{nameof(NewOnSetup)}\" processing. Could not create instance with no-args constructor for type {field.FieldType}. (Field={field}, Role={EnglishRoleName})");
            }
        else
            try {
                newValue = cloneMethod.Invoke(currentValue, null)!;
            }
            catch (Exception e) {
                VentLogger.Exception(e);
                throw new ArgumentException($"Error during \"{nameof(NewOnSetup)}\" processing. Could not clone original instance for type {field.FieldType}. (Field={field}, Role={EnglishRoleName})");
            }
        field.SetValue(this, newValue);
    }

    private void CreateCooldown(FieldInfo fieldInfo)
    {
        Cooldown? value = (Cooldown)fieldInfo.GetValue(this);
        Cooldown setValue = value == null ? new Cooldown() : value.Clone();
        value?.TimeRemaining();
        fieldInfo.SetValue(this, setValue);
    }

    private void CreateOptional(FieldInfo fieldInfo)
    {
        ConstructorInfo GetConstructor(Type[] parameters) => AccessTools.Constructor(fieldInfo.FieldType, parameters);

        object? optional = fieldInfo.GetValue(this);
        ConstructorInfo constructor = GetOptionalConstructor(fieldInfo, optional == null);
        object? setValue = constructor.Invoke(optional == null ? Array.Empty<object>() : new[] { optional });
        fieldInfo.SetValue(this, setValue);
    }

    private ConstructorInfo GetOptionalConstructor(FieldInfo info, bool isNull)
    {
        if (isNull) return AccessTools.Constructor(info.FieldType, Array.Empty<Type>());
        return info.FieldType.GetConstructors().First(c =>
            c.GetParameters().SelectWhere(p => p.ParameterType, t => t!.IsGenericType).Any(tt =>
                tt!.GetGenericTypeDefinition().IsAssignableTo(typeof(Optional<>))));
    }

    /// <summary>
    /// This method is called when the role class is Instantiated (during role selection),
    /// thus allowing modifications to the specific player attached to this role
    /// </summary>
    /// <param name="player">The player assigned to this role</param>
    protected virtual void Setup(PlayerControl player) { }

    protected virtual void PostSetup() {}

    /// <summary>
    /// Forced method that allows CustomRoles to provide unique definitions for themselves
    /// </summary>
    /// <param name="roleModifier">Automatically supplied RoleFactory used for class specifications</param>
    /// <returns>Provided <b>OR</b> new RoleFactory</returns>
    protected abstract RoleModifier Modify(RoleModifier roleModifier);

    public GameOptionBuilder GetGameOptionBuilder()
    {
        GameOptionBuilder b = GetBaseBuilder();
        if (RoleFlags.HasFlag(RoleFlag.RemoveRoleMaximum)) return RegisterOptions(b);

       b = b.SubOption(s => s.Name(RoleTranslations.MaximumText)
                .Key("Maximum")
                .AddIntRange(1, 15)
                .Bind(val => this.Count = (int)val)
                .ShowSubOptionPredicate(v => 1 < (int)v)
                .SubOption(subsequent => subsequent
                    .Name(RoleTranslations.SubsequentChanceText)
                    .Key("Subsequent Chance")
                    .AddIntRange(10, 100, 10, 0, "%")
                    .BindInt(v => AdditionalChance = v)
                    .Build())
                .Build());

        return RegisterOptions(b);
    }

    protected virtual GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        Localizer localizer = Localizer.Get(callingAssembly);
        string qualifier = $"Roles.{EnglishRoleName}.RoleName";

        return optionStream
            .LocaleName(qualifier)
            .Key(EnglishRoleName)
            .Color(RoleColor)
            .Description(localizer.Translate($"Roles.{EnglishRoleName}.Blurb"))
            .IsHeader(true);
    }

    [SuppressMessage("ReSharper", "RemoveRedundantBraces")]
    private GameOptionBuilder GetBaseBuilder()
    {
        if (!RoleFlags.HasFlag(RoleFlag.RemoveRolePercent))
        {
            return new GameOptionBuilder()
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
                .BindInt(val => this.Chance = val)
                .AddIntRange(0, 100, RoleFlags.HasFlag(RoleFlag.IncrementChanceByFives) ? 5 : 10, 0, "%")
                .ShowSubOptionPredicate(value => ((int)value) > 0);
        }

        string onText = GeneralOptionTranslations.OnText;
        string offText = GeneralOptionTranslations.OffText;

        if (RoleFlags.HasFlag(RoleFlag.TransformationRole))
        {
            onText = GeneralOptionTranslations.ShowText;
            offText = GeneralOptionTranslations.HideText;
        }

        return new GameOptionBuilder()
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
                .Value(v => v.Text(onText).Value(true).Color(Color.cyan).Build())
                .Value(v => v.Text(offText).Value(false).Color(Color.red).Build())
                .ShowSubOptionPredicate(b => (bool)b);
    }


    public override string ToString()
    {
        return this.RoleName;
    }

    public class RoleModifier
    {
        private AbstractBaseRole myRole;

        public RoleModifier(AbstractBaseRole role)
        {
            this.myRole = role;
        }

        public RoleModifier DesyncRole(RoleTypes? desyncRole)
        {
            myRole.DesyncRole = desyncRole;
            return this;
        }

        public RoleModifier VanillaRole(RoleTypes vanillaRole)
        {
            myRole.VirtualRole = vanillaRole;
            return this;
        }

        public RoleModifier SpecialType(SpecialType specialType)
        {
            myRole.SpecialType = specialType;
            return this;
        }

        public RoleModifier Faction(IFaction factions)
        {
            myRole.Faction = factions;
            return this;
        }

        public RoleModifier CanVent(bool canVent)
        {
            myRole.BaseCanVent = canVent;
            return this;
        }


        public RoleModifier Subrole(bool isSubrole)
        {
            myRole.IsSubrole = isSubrole;
            return this;
        }

        public RoleModifier OptionOverride(Override option, object? value, Func<bool>? condition = null)
        {
            myRole.roleSpecificGameOptionOverrides.Add(new GameOptionOverride(option, value, condition));
            return this;
        }

        public RoleModifier OptionOverride(Override option, Func<object> valueSupplier, Func<bool>? condition = null)
        {
            myRole.roleSpecificGameOptionOverrides.Add(new GameOptionOverride(option, valueSupplier, condition));
            return this;
        }

        public RoleModifier OptionOverride(GameOptionOverride @override)
        {
            myRole.roleSpecificGameOptionOverrides.Add(@override);
            return this;
        }

        public RoleModifier RoleName(string adjustedName)
        {
            myRole.EnglishRoleName = adjustedName;
            return this;
        }

        public RoleModifier RoleColor(string htmlColor)
        {
            if (ColorUtility.TryParseHtmlString(htmlColor, out Color color))
                myRole.RoleColor = color;
            return this;
        }

        public RoleModifier RoleColor(Color color)
        {
            myRole.RoleColor = color;
            return this;
        }

        /// <summary>
        /// Roles that are "Linked" to this role. What this means:
        /// ANY role you put in here will have its options become a sub-option of this role.
        /// Additionally, you DO NOT (AND SHOULD NOT) register said role in your addon. The role will be automatically hooked in by being a member
        /// of this function. If you happen to do so probably nothing will go wrong, but it's undefined behaviour and should be avoided.
        /// <br/><br/>
        /// If you're looking to change the options from % chance to "Show" / "Hide" refer to <see cref="RoleFlags"/> on the child role.
        /// </summary>
        /// <param name="roles">The roles linked to this role</param>
        /// <returns>this role modifier</returns>
        public RoleModifier LinkedRoles(params CustomRole[] roles)
        {
            myRole.LinkedRoles.AddRange(roles);
            return this;
        }

        public RoleModifier RoleFlags(RoleFlag roleFlags)
        {
            myRole.RoleFlags = roleFlags;
            return this;
        }
    }

    public abstract class RoleEditor
    {
        internal AbstractBaseRole FrozenRole { get; }
        internal AbstractBaseRole ModdedRole = null!;
        internal CustomRole? RoleInstance;

        internal RoleEditor(AbstractBaseRole baseRole)
        {
            this.FrozenRole = baseRole;
        }

        internal AbstractBaseRole StartLink()
        {
            _editors.Clear();
            _editors.Add(this);
            this.ModdedRole = (AbstractBaseRole)Activator.CreateInstance(FrozenRole.GetType())!;
            this.ModdedRole.Editor = this;
            _editors.Clear();
            this.SetupActions();
            OnLink();
            return ModdedRole;
        }

        internal RoleEditor Instantiate(CustomRole role, PlayerControl player)
        {
            RoleEditor cloned = (RoleEditor)this.MemberwiseClone();
            cloned.RoleInstance = role;
            cloned.HookSetup(player);
            return cloned;
        }

        public virtual void HookSetup(PlayerControl myPlayer) { }

        public virtual RoleModifier HookModifier(RoleModifier modifier) {
            return modifier;
        }

        public virtual GameOptionBuilder HookOptions(GameOptionBuilder optionStream) {
            return optionStream;
        }
        
        public virtual void AddAction(RoleAction action)
        {
            FrozenRole.roleActions[action.ActionType].Add(action);
        }

        public abstract void OnLink();

        private void PatchHook(object?[] args, ModifiedAction action, MethodInfo baseMethod)
        {
            if (action.Behaviour is ModifiedBehaviour.PatchBefore)
            {
                object? result = action.method.InvokeAligned(args);
                if (action.method.ReturnType == typeof(bool) && (result == null || (bool)result))
                    baseMethod.InvokeAligned(args);
                return;
            }

            baseMethod.InvokeAligned(args);
            action.method.InvokeAligned(args);
        }

        

        private void SetupActions()
        {
            this.GetType().GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(method => method.GetCustomAttributes<RoleActionAttribute>().Select(a => (a, method)))
                .Where(t => t.a.Subclassing || t.method.DeclaringType == this.GetType())
                .Select(t => t.Item1 is ModifiedActionAttribute modded ? new ModifiedAction(modded, t.method) : new RoleAction(t.Item1!, t.method))
                .Do(action =>
                {
                    if (action is not ModifiedAction modded) ModdedRole.AddRoleAction(action);
                    else {
                        List<RoleAction> currentActions = ModdedRole.roleActions.GetValueOrDefault(action.ActionType, new List<RoleAction>());

                        switch (modded.Behaviour)
                        {
                            case ModifiedBehaviour.Replace:
                                currentActions.Clear();
                                currentActions.Add(modded);
                                break;
                            case ModifiedBehaviour.Addition:
                                currentActions.Add(modded);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        ModdedRole.roleActions[action.ActionType] = currentActions;
                    }
                });
        }
    }

    public class BasicRoleEditor : RoleEditor
    {
        public BasicRoleEditor(AbstractBaseRole baseRole) : base(baseRole)
        {
        }

        public override void OnLink()
        {
        }
    }
}