using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lotus.API.Odyssey;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;

namespace Lotus.Managers.Titles;

public class CustomTitle
{
    private static Regex _commaRegex = new("( *, *)");
    private static Regex _tagRegex = new("(<([^>]*)(=?[^>]*?)>([^<]*)<\\/\\2>)");

    public Component? Prefix { get; set; }
    public Component? Suffix { get; set; }
    public Component? UpperText { get; set; }
    public Component? LowerText { get; set; }
    public Component? Name { get; set; }


    public string ApplyTo(string playerName, bool nameOnly = false)
    {
        if (AmongUsClient.Instance != null && !AmongUsClient.Instance.AmHost) return playerName;
        if (Game.State is not GameState.InLobby) return playerName;

        string? prefix = Prefix?.Generate();
        string? suffix = Suffix?.Generate();

        prefix = prefix == null ? "" : prefix + (Prefix?.Spaced ?? false ? " " : "");
        suffix = suffix == null ? "" : (Suffix?.Spaced ?? false ? " " : "") + suffix;

        playerName = Name?.GenerateName(playerName, nameOnly) ?? playerName;

        if (nameOnly) return $"{prefix}{playerName}{suffix}";

        string? upperText = UpperText?.Generate();
        string? lowerText = LowerText?.Generate();

        upperText = upperText == null ? "" : upperText + "\n";
        lowerText = lowerText == null ? "" : "\n" + lowerText;
        return $"{upperText}{prefix}{playerName}{suffix}{lowerText}";
    }

    public class Component
    {
        public string? Size { get; set; }
        public string? Text { get; set; }
        public string? Color { get; set; }
        public string? Gradient { get; set; }
        public int GradientDegree { get; set; } = -1;
        public bool Spaced { get; set; } = true;
        internal ColorGradient? InternalGradient;
        internal Color? InternalColor;

        public string Generate()
        {
            if (Text == null)
            {
                VentLogger.Warn("Could not generate title component. Text for component cannot be null!", "CustomTitleGenerator");
                return "";
            }

            List<(string rich, string richValue, string text)> tuples = _tagRegex.Matches(Text).Select(m => m.Groups).Select(g => (g[2].Value, g[3].Value, g[4].Value)).ToList();
            string modifiedText = _tagRegex.Replace(Text, "⚡");

            if (GradientDegree == -1) GradientDegree = Math.Max(1, modifiedText.Count(c => c != '⚡') / 9);

            if (InternalGradient == null && Gradient != null) InternalGradient = CreateGradient(Gradient);
            if (InternalGradient != null) return ApplySize(InternalGradient.Apply(modifiedText, GradientDegree), tuples);

            if (Color == null) return ApplySize(modifiedText, tuples);
            InternalColor = ParseToColor(Color);
            return ApplySize(InternalColor == UnityEngine.Color.white ? Text : InternalColor.Value.Colorize(modifiedText), tuples);
        }

        internal string GenerateName(string name, bool ignoreSize = false)
        {
            if (GradientDegree == -1) GradientDegree = Math.Max(1, name.Length / 9);
            if (InternalGradient == null && Gradient != null) InternalGradient = CreateGradient(Gradient);
            if (InternalColor == null && Color != null) InternalColor = ParseToColor(Color);
            if (InternalGradient != null) name = InternalGradient.Apply(name, GradientDegree);
            else if (InternalColor != null && InternalColor != UnityEngine.Color.white) name = InternalColor.Value.Colorize(name);
            return ApplySize(name, new List<(string rich, string richValue, string text)>(), ignoreSize);
        }

        private string ApplySize(string name, List<(string rich, string richValue, string text)> tuples, bool ignoreSize = false)
        {
            string text = Size != null && !ignoreSize ? $"<size={Size}>{name}</size>" : name;
            tuples.ForEach(t =>
            {
                string html = $"<{t.rich}{t.richValue}>{t.text}</{t.rich}>";
                text = text.ReplaceN("⚡", html, 1);
            });
            return text;
        }
    }

    private static ColorGradient CreateGradient(string gradient)
    {
        Color[] colors = _commaRegex.Split(gradient)
            .Where(s => s is not (" " or "") && !s.Contains(','))
            .Select(ParseToColor)
            .ToArray();

        return new ColorGradient(colors);
    }

    private static Color ParseToColor(string input)
    {
        Color? c = input.ToColor();
        if (c != null) return c.Value;

        VentLogger.Warn($"Could not parse to color {c}", "CustomTitleGenerator");
        return Color.white;

    }

    public override string ToString()
    {
        return ApplyTo("[PlayerName]").Replace("\n", "\\n");
    }
}