using UnityEngine;
using VentLib.Utilities.Attributes;

namespace TOHTOR.GUI.Menus.OptionsMenu;

[LoadStatic]
internal class OptionMenuResources
{
    public static Sprite BackgroundSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(BackgroundSprite));
    public static Sprite ButtonOnSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ButtonOnSprite));
    public static Sprite ButtonOffSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ButtonOffSprite));

    public static Sprite GeneralButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(GeneralButton));
    public static Sprite GraphicsButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(GraphicsButton));
    public static Sprite SoundButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(SoundButton));
    public static Sprite VentLibButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(VentLibButton));
    public static Sprite AddonsButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(AddonsButton));
    public static Sprite ReturnButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ReturnButton));
    public static Sprite ExitButton => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ExitButton));

    const int ButtonPpu = 600;

    static OptionMenuResources()
    {
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(BackgroundSprite), "TOHTOR.assets.Settings.MenuBackground.png", 525);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ButtonOnSprite), "TOHTOR.assets.Settings.SelectButton.png", 450);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ButtonOffSprite), "TOHTOR.assets.Settings.UnselectButton.png", 450);

        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GeneralButton), "TOHTOR.assets.Settings.GeneralBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GraphicsButton), "TOHTOR.assets.Settings.GraphicBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(SoundButton), "TOHTOR.assets.Settings.SoundBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(VentLibButton), "TOHTOR.assets.Settings.VentLibBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(AddonsButton), "TOHTOR.assets.Settings.AddonBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ReturnButton), "TOHTOR.assets.Settings.ReturnBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ExitButton), "TOHTOR.assets.Settings.ExitBlank.png", ButtonPpu);
    }

}