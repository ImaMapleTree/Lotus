using System.Collections.Generic;
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

    public const float MaxPlayerSpeed = 3f;

    public static string[] ColorNames = new[]
    {
        "Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown", "Cyan", "Lime",
        "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
    };


    public const string Infinity = "∞";

    public static class Palette
    {
        public static Color InfinityColor = new(0.77f, 0.71f, 1f);
        public static Color NeutralColor = new(1f, 0.67f, 0.11f);
        public static Color PassiveColor = new(1f, 0.87f, 0.91f);
        public static Color KillingColor = new(1f, 0.27f, 0.18f);

        public static Color GeneralColor1 = new(0.15f, 0.27f, 0.33f);
        public static Color GeneralColor2 = new(0.16f, 0.62f, 0.56f);
        public static Color GeneralColor3 = new(0.91f, 0.77f, 0.42f);
        public static Color GeneralColor4 = new(0.96f, 0.64f, 0.38f);
        public static Color GeneralColor5 = new(0.91f, 0.44f, 0.32f);


        public static Color InvalidUsage = new(1f, 0.67f, 0.11f);
    }

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

    public static Dictionary<string, string> Pets = new()
    {
        { "Random", "Random" },
        { "Bedcrab", "pet_Bedcrab" },
        { "Breb", "pet_BredPet" },
        { "Brant", "pet_YuleGoatPet" },
        { "Bushfriend", "pet_Bush" },
        { "Charles Chopper", "pet_Charles" },
        { "Chewie", "pet_ChewiePet" },
        { "Clank", "pet_clank" },
        { "Coalton", "pet_coaltonpet" },
        { "Deitied Guy", "pet_Cube" },
        { "Doggy", "pet_Doggy" },
        { "E. Rose", "pet_Ellie" },
        { "Frankendog", "pet_frankendog" },
        { "Ghost", "pet_Ghost" },
        { "Glitch Pet", "pet_test" },
        { "Guilty Spark", "pet_GuiltySpark" },
        { "H. Stickmin", "pet_Stickmin" },
        { "Hammy", "pet_HamPet" },
        { "Hampton", "pet_Hamster" },
        { "Headslug", "pet_Allien" },
        { "Poro", "pet_poro" },
        { "Magmate", "pet_Lava" },
        { "Crewmate", "pet_Crewmate" },
        { "Pouka", "pet_Pouka" },
        { "Ro-Bot", "pet_Robot" },
        { "Snowball", "pet_Snow" },
        { "Squig", "pet_Squig" },
        { "Nugget", "pet_nuggetPet" },
        { "Toppat Chopper", "pet_Charles_Red" },
        { "UFO", "pet_UFO" },
        { "Worm", "pet_Worm" },
    };
}