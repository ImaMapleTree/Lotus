using UnityEngine;
using VentLib.Utilities.Attributes;

namespace Lotus.GUI.Menus.OptionsMenu;

[LoadStatic]
internal class OptionMenuResources
{
    public static Sprite OptionsBackgroundSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(OptionsBackgroundSprite));
    public static Sprite ModUpdaterBackgroundSprite => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ModUpdaterBackgroundSprite));
    
    public static Sprite ProgressBarFull => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ProgressBarFull));
    public static Sprite ProgressBarMask => PersistentAssetLoader.GetSprite(nameof(OptionMenuResources) + nameof(ProgressBarMask));
    
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
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(OptionsBackgroundSprite), "Lotus.assets.Settings.MenuBackground.png", 525);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ModUpdaterBackgroundSprite), "Lotus.assets.Settings.UpdateMenuBackground.png", 400);

        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ProgressBarFull), "Lotus.assets.Settings.ProgressBarFill.png", 800);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ProgressBarMask), "Lotus.assets.Settings.ProgressBarMask.png", 800);
        
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ButtonOnSprite), "Lotus.assets.Settings.SelectButton.png", 450);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ButtonOffSprite), "Lotus.assets.Settings.UnselectButton.png", 450);

        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GeneralButton), "Lotus.assets.Settings.GeneralBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(GraphicsButton), "Lotus.assets.Settings.GraphicBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(SoundButton), "Lotus.assets.Settings.SoundBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(VentLibButton), "Lotus.assets.Settings.VentLibBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(AddonsButton), "Lotus.assets.Settings.AddonBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ReturnButton), "Lotus.assets.Settings.ReturnBlank.png", ButtonPpu);
        PersistentAssetLoader.RegisterSprite(nameof(OptionMenuResources) + nameof(ExitButton), "Lotus.assets.Settings.ExitBlank.png", ButtonPpu);
    }

}