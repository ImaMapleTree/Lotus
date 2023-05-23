using System;
using Lotus.Logging;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Options.Client;

[Localized(ModConstants.Options)]
public class VideoOptions
{
    public static object[] FpsLimits = { 24, 30, 60, 75, 120, 144, 240, Int32.MaxValue };

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
        OptionManager optionManager = OptionManager.GetManager();
        fpsOption = new GameOptionBuilder()
            .Key("Max Framerate")
            .Name("Max Framerate")
            .Description("Maximum Framerate for the Application")
            .IOSettings(s => s.UnknownValueAction = ADEAnswer.Allow)
            .Values(2, FpsLimits)
            .BindInt(i =>
            {
                optionManager.DelaySave();
                Application.targetFrameRate = _targetFps = i;
                Async.Schedule(() => Application.targetFrameRate = _targetFps, 1f);
            })
            .BuildAndRegister();
    }


}