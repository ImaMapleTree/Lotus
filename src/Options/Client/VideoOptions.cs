using System;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities.Extensions;

namespace Lotus.Options.Client;

[Localized("Options")]
public class VideoOptions
{
    public static object[] FpsLimits = { 24, 30, 60, 120, 144, 240, 265, Int32.MaxValue };

    public int TargetFps
    {
        get => _targetFps;
        set
        {
            int index = FpsLimits.IndexOf(i => (int)i == value);
            fpsOption.SetValue(index != -1 ? index : 2);
            _targetFps = value;
        }
    }

    private int _targetFps;

    private GameOption fpsOption;

    public VideoOptions()
    {
        fpsOption = new GameOptionBuilder()
            .Key("Max Framerate")
            .Name("Max Framerate")
            .Description("Maximum Framerate for the Application")
            .IOSettings(s => s.UnknownValueAction = ADEAnswer.Allow)
            .Values(2, FpsLimits)
            .BindInt(i => _targetFps = i)
            .BuildAndRegister();
    }


}