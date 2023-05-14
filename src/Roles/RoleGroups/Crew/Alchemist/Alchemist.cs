using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Impl;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Overrides;
using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Roles.RoleGroups.Crew.Potions;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Roles.Internals;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;

namespace Lotus.Roles.RoleGroups.Crew;

[Localized("Roles.Alchemist")]
public partial class Alchemist: Crewmate
{
    private const float AlchemistFixedUpdate = 0.1f;
    [Localized("FoundIngredientText")]
    private static string _foundString = "Found:";
    [Localized("CraftingPotionText")]
    private static string _craftingString = "Crafting:";

    public static HashSet<IAlchemyIngredient> GlobalIngredients = new();
    [NewOnSetup] public HashSet<IAlchemyIngredient> LocalIngredients;

    private DateTime lastRun = DateTime.Now;

    public bool IsProtected;
    public float VisionMod;
    public int ExtraVotes;
    public bool QuickFixSabotage;

    protected override void Setup(PlayerControl player) => VisionMod = AUSettings.CrewLightMod();

    protected override void PostSetup()
    {
        Ingredient.AllIngredients.ForEach(i => heldIngredients[i] = 0);
        Craftables = PotionConstructors.Select(ctor => ctor(baseCatalystAmount))
            .Where(p => !bannedPotions.Contains(p.GetType()))
            .Cast<ICraftable>()
            .ToList();
    }

    [UIComponent(UI.Counter, ViewMode.Replace)]
    private string RemoveCounter() => "";

    [UIComponent(UI.Role, ViewMode.Absolute)]
    private string IngredientDisplay() => craftingMode ? CraftingRoleDisplay() : progressBar != "" ? progressBar : heldIngredients
            .Select(kvp => kvp.Key.Color.Colorize(kvp.Key.Symbol) + $"{kvp.Value}")
            .Join(delimiter: " ");

    [UIComponent(UI.Text, ViewMode.Overriden)]
    private string TextDisplay()
    {
        if (craftingMode) return CraftingTextDisplay();
        if (currentlyCrafting != null) return _craftingString + " " + currentlyCrafting.Color().Colorize(currentlyCrafting.Name());
        if (collectableIngredient != null) return _foundString + " " + collectableIngredient.Color().Colorize(collectableIngredient.Name());
        return HeldCraftable != null ? HeldCraftable.Color().Colorize(HeldCraftable.Name()) : "";
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(1f, 0.69f, 0.56f))
            .OptionOverride(Override.CrewLightMod, () => VisionMod);
}