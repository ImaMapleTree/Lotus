using System;
using System.Reflection;
using Lotus.GUI;
using Lotus.Utilities;
using TMPro;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;
using Object = UnityEngine.Object;

namespace Lotus.Roles2.GUI;

public class RoleButton: RoleButtonEditor, ICloneable<RoleButton>
{
    protected Sprite Sprite { get => lazySprite?.Get() ?? _sprite; private set => _sprite = value; }
    internal bool IsOverriding = true;

    private AssetRegistry assetRegistry;
    private UnityOptional<ActionButton> underlyingButton;
    private ActionButton setButton;

    private Func<ActionButton> buttonSupplier;
    private Cooldown? boundCooldown;
    private Func<int>? usesSupplier;

    private Sprite originalSprite;

    private Sprite _sprite = null!;

    private LazySprite? lazySprite;

    private string text;

    public RoleButton(AssetRegistry assetRegistry, Func<ActionButton> buttonSupplier)
    {
        this.assetRegistry = assetRegistry;
        this.underlyingButton = UnityOptional<ActionButton>.Of(buttonSupplier.Invoke());
        underlyingButton.IfPresent(b => this.Sprite = b.graphic.sprite);
        underlyingButton.IfPresent(b => this.text = b.buttonLabelText.text);
        this.buttonSupplier = buttonSupplier;
    }

    public RoleButton BindCooldown(Cooldown cooldown)
    {
        boundCooldown = cooldown;
        ActionButton button = GetButton();

        if (button.cooldownTimerText == null)
        {
            button.cooldownTimerText = Object.Instantiate(HudManager.Instance.KillButton.cooldownTimerText, button.gameObject.transform);
            button.cooldownTimerText.transform.localPosition += new Vector3(0, 0, -20);
        }

        return this;
    }

    public RoleButton BindUses(Func<int> usesSupplier)
    {
        ActionButton button = GetButton();
        if (button.usesRemainingSprite == null)
        {
            button.usesRemainingSprite = Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingSprite, button.gameObject.transform);
            button.usesRemainingSprite.transform.localPosition += new Vector3(0, 0, -34f);
            button.usesRemainingText = Object.Instantiate(HudManager.Instance.AbilityButton.usesRemainingText, button.usesRemainingSprite.gameObject.transform);
            button.usesRemainingText.gameObject.SetActive(true);
        }

        this.usesSupplier = usesSupplier;
        return this;
    }

    internal void CompleteLoad() => setButton = GetButton();

    public RoleButton Default(bool resetCustom)
    {
        IsOverriding = resetCustom;
        return this;
    }

    public RoleButton LoadSpriteOnButton(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (!assetRegistry.HasEntry(resourcePath)) assetRegistry.CreateEntry(resourcePath).Sprite(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly);
        lazySprite = assetRegistry.GetLazySprite(resourcePath);
        return SetSprite(lazySprite);
    }

    public RoleButton LoadSpriteAndTextOnButton(string text, string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (!assetRegistry.HasEntry(resourcePath)) assetRegistry.CreateEntry(resourcePath).Sprite(resourcePath, pixelsPerUnit, linear, mipMapLevels, assembly);
        lazySprite = assetRegistry.GetLazySprite(resourcePath);
        GetButton().buttonLabelText.text = this.text = text;
        return SetSprite(lazySprite);
    }

    public RoleButton SetText(string text)
    {
        GetButton().buttonLabelText.text = this.text = text;
        return this;
    }

    public RoleButton SetSprite(string refName) => SetSprite(assetRegistry.GetLazySprite(refName));

    public RoleButton SetSprite(LazySprite sprite)
    {
        lazySprite = sprite;
        GetButton().graphic.sprite = Sprite = lazySprite.Get();
        return this;
    }

    public RoleButton RevertSprite()
    {
        if (originalSprite != null) GetButton().graphic.sprite = originalSprite;
        return this;
    }

    public Sprite GetSprite() => Sprite;

    public string GetText() => text;

    public ActionButton GetButton()
    {
        if (!ReferenceEquals(setButton, null)) return setButton;
        ActionButton button = underlyingButton.OrElseSet(buttonSupplier);
        if (originalSprite == null) originalSprite = button.graphic.sprite;
        return button;
    }

    public void Update()
    {
        ActionButton button = GetButton();

        if (usesSupplier != null)
        {
            int uses = usesSupplier();
            if (uses is <= -1 or int.MaxValue) button.SetInfiniteUses();
            else
            {
                button.SetUsesRemaining(uses);
                if (uses == 0) button.SetDisabled();
            }

            if (uses == 0) button.SetDisabled();
        }

        button.buttonLabelText.text = text;
        if (boundCooldown == null) return;
        float cooldown = boundCooldown.TimeRemaining();
        button.SetCoolDown(cooldown, boundCooldown.Duration);
    }

    internal void ForceUpdate()
    {
        ActionButton button = GetButton();
        button.buttonLabelText.text = text;
        button.graphic.sprite = GetSprite();
    }

    public RoleButton Clone() => (RoleButton)this.MemberwiseClone();
}

public interface RoleButtonEditor
{
    public RoleButton BindCooldown(Cooldown cooldown);

    public RoleButton BindUses(Func<int> usesSupplier);

    public RoleButton LoadSpriteOnButton(string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null);

    public RoleButton LoadSpriteAndTextOnButton(string text, string resourcePath, float pixelsPerUnit, bool linear = false, int mipMapLevels = 0, Assembly? assembly = null);

    public RoleButton Default(bool resetCustom);
}