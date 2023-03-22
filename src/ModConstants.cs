using UnityEngine;

namespace TOHTOR;

public static class ModConstants
{
    public static Color HColor1 = new Color(0.03f, 0.53f, 0.01f);
    public static Color HColor2 = new Color(0.71f, 0.33f, 0f);

    public const int MaxPlayers = 15;

    // Minimum distance for arrow to show (versus dot)
    public const float ArrowActivationMin = 3;

    public const float DynamicNameTimeBetweenRenders = 0.25f;

    public const double RoleFixedUpdateCooldown = 0.25;

    public const float DeriveDelayMultiplier = 0.0003f;
    public const float DeriveDelayFlatValue = 0.4f;

    public const int RecursiveDepthLimit = 200;

    public static string[] ColorNames = new[]
    {
        "Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown", "Cyan", "Lime",
        "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
    };

    public static class DeathNames
    {
        public const string Killed = "Killed";

        public const string Suicide = "Suicide";

        public const string Exiled = "Exiled";

        public const string Bombed = "Bombed";

        public const string Bitten = "Bitten";

        public const string Cursed = "Cursed";

        public const string Incinerated = "Incinerated";
    }
}