using System.Collections.Generic;
using System.Linq;
using Lotus.Logging;
using Lotus.Roles2.Attributes;
using Lotus.Roles2.Interfaces;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.Roles2.GUI;

public class GUIProvider<T>: GUIProvider where T: RoleGUI
{
    public virtual void HandleGUIComponent(T guiComponent) => base.HandleGUIComponent(guiComponent);

    public override void HandleGUIComponent(RoleGUI guiComponent)
    {
        if (guiComponent is T t) HandleGUIComponent(t);
        else base.HandleGUIComponent(guiComponent);
    }
}

[InstantiateOnSetup(true, true)]
public class GUIProvider: IInstantiatedComponentAware<RoleGUI>, IDefinitionAware
{
    protected AssetRegistry AssetRegistry { get; } = new();
    public RoleDefinition Definition { get; private set; } = null!;
    protected List<RoleGUI> GUIComponents = null!;
    internal bool Initialized = false;

    [SetupInjected]
    private List<RoleButton> modifiedButtons = new();

    private List<RoleGUI> updateComponents;

    public RoleButton PetButton = null!;

    public RoleButton UseButton = null!;

    public RoleButton ReportButton = null!;

    public RoleButton VentButton = null!;

    public RoleButton KillButton = null!;

    public RoleButton AbilityButton = null!;

    public virtual void HandleGUIComponent(RoleGUI guiComponent)
    {
        DevLogger.Log($"Handing GUI component: {this.GetHashCode()} | {guiComponent.GetHashCode()}");
        guiComponent.SpriteBindings(AssetRegistry);
        if (PetButton != null!)
        {
            RoleButton petButton = guiComponent.PetButton(PetButton.Clone());
            if (petButton.IsOverriding) PetButton = RegisterModification(petButton);
        }

        if (UseButton != null!)
        {
            RoleButton useButton = guiComponent.UseButton(UseButton.Clone());
            if (useButton.IsOverriding) UseButton = RegisterModification(useButton);
        }

        if (VentButton != null!)
        {
            RoleButton ventButton = guiComponent.VentButton(VentButton.Clone());
            if (ventButton.IsOverriding) VentButton = RegisterModification(ventButton);
        }

        if (ReportButton != null!)
        {
            RoleButton reportButton = guiComponent.ReportButton(ReportButton.Clone());
            if (reportButton.IsOverriding) ReportButton = RegisterModification(reportButton);
        }

        if (KillButton != null!)
        {
            RoleButton killButton = guiComponent.KillButton(KillButton.Clone());
            if (killButton.IsOverriding) KillButton = RegisterModification(killButton);
        }

        if (AbilityButton != null!)
        {
            RoleButton abilityButton = guiComponent.AbilityButton(AbilityButton.Clone());
            if (abilityButton.IsOverriding) AbilityButton = RegisterModification(abilityButton);
        }
    }

    public virtual void InitializeButtons()
    {
        Initialized = true;
        PetButton = new RoleButton(AssetRegistry, () => AmongUsButtonSpriteReferences.PetButton);
        UseButton = new RoleButton(AssetRegistry, () => AmongUsButtonSpriteReferences.UseButton);
        ReportButton = new RoleButton(AssetRegistry, () => AmongUsButtonSpriteReferences.ReportButton);
        VentButton = new RoleButton(AssetRegistry, () => AmongUsButtonSpriteReferences.VentButton);
        AbilityButton = new RoleButton(AssetRegistry, () => AmongUsButtonSpriteReferences.AbilityButton);
        AbilityButton.GetButton().SetInfiniteUses();
        KillButton = new RoleButton(AssetRegistry, () => AmongUsButtonSpriteReferences.KillButton);
    }

    internal void Start()
    {
        Definition.MyPlayer.Data.Role.InitializeAbilityButton();
        InitializeButtons();
        GUIComponents.ForEach(HandleGUIComponent);
        modifiedButtons.ForEach(b => b.CompleteLoad());
    }

    protected RoleButton RegisterModification(RoleButton button)
    {
        modifiedButtons.Add(button);
        return button;
    }

    public void Update()
    {
        modifiedButtons.ForEach(b => b.Update());
        updateComponents.ForEach(c => c.UpdateGUI(this));
    }

    internal void ForceUpdate()
    {
        modifiedButtons.ForEach(b => b.ForceUpdate());
    }

    public LazySprite GetLazySprite(string refName) => AssetRegistry.GetLazySprite(refName);

    public Sprite GetSprite(string refName) => AssetRegistry.GetSprite(refName);

    public void SetRoleDefinition(RoleDefinition definition) => Definition = definition;

    public IRoleComponent Instantiate(SetupHelper setupHelper, PlayerControl player) => setupHelper.Clone(this);

    public virtual void ReceiveTargetInstantiatedComponents(List<RoleGUI> components)
    {
        GUIComponents = components;
        updateComponents = GUIComponents.Where(g => g.GetType().GetMethod(nameof(RoleGUI.UpdateGUI), AccessFlags.AllAccessFlags) != null).ToList();
    }
}