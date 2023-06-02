using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.Crew.Ingredients;
using Lotus.Roles.RoleGroups.Crew.Potions;
using Lotus.Extensions;
using UnityEngine;
using VentLib.Logging;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Crew;

public partial class Alchemist
{
    private bool craftingMode;
    private int craftingPage;

    public ICraftable? HeldCraftable;
    private ICraftable? currentlyCrafting;
    private string progressBar = "";
    private bool advancePage;
    private bool waitForReleaseFlag;

    [UIComponent(UI.Name, ViewMode.Absolute, GameState.Roaming)]
    private string CraftingNameDisplay()
    {
        if (!craftingMode) return MyPlayer.name;
        ICraftable craftable = Craftables[craftingPage];
        return RoleUtils.Counter(craftingPage + 1, Craftables.Count, Color.white) + " " + craftable.Color().Colorize(craftable.Name());
    }

    private string CraftingRoleDisplay()
    {
        ICraftable craftable = Craftables[craftingPage];
        return craftable.Ingredients().Entries().Select(kvp => kvp.Key + kvp.Value.ToString()).Join(delimiter: " ");
    }

    private string CraftingTextDisplay()
    {
        ICraftable craftable = Craftables[craftingPage];

        string ColorIngredient(KeyValuePair<IngredientInfo, int> kvp)
        {
            string value;
            if (!craftable.Ingredients().ContainsKey(kvp.Key)) value = kvp.Value.ToString();
            else
            {
                Color valueColor = craftable.Ingredients()[kvp.Key] <= kvp.Value ? Color.green : Color.red;
                value = valueColor.Colorize(kvp.Value.ToString());
            }

            return kvp.Key + value;
        }


        return heldIngredients.Select(ColorIngredient).Join(delimiter: " ");
    }

    private bool CanCraft(ICraftable craftable)
    {
        bool eligible = craftable.Ingredients().Entries().All(ev => heldIngredients.GetValueOrDefault(ev.Key) >= ev.Value);
        VentLogger.Trace($"Checking Crafting Eligibility of {craftable.Name()} => {eligible}");
        return eligible;
    }

    // Switches the crafting page after being held down twice (0.8 seconds)
    [RoleAction(RoleActionType.OnHoldPet)]
    private void SwitchCraftingPage(int times)
    {
        if (waitForReleaseFlag) return;
        if (HeldCraftable != null)
        {
            progressBar = RoleUtils.ProgressBar(times, 3, HeldCraftable.Color());
            if (times != 4) return;
            if (HeldCraftable.Use(MyPlayer)) HeldCraftable = null;
            progressBar = "";
            waitForReleaseFlag = true;
            return;
        }

        if (currentlyCrafting != null) progressBar = RoleUtils.ProgressBar(times - 2, 3, RoleColor);

        switch (times)
        {
            case 1 when craftingMode:
                advancePage = true;
                break;
            case 2:
                ToggleCrafting();
                if (currentlyCrafting != null) progressBar = RoleUtils.ProgressBar(times - 2, 3, RoleColor);
                if (currentlyCrafting != null) VentLogger.Trace($"{MyPlayer.GetNameWithRole()} => Currently Crafting: {currentlyCrafting.Name()}");
                advancePage = false;
                break;
            case 6:
                FinalizeCrafting();
                break;
        }
    }


    // Toggles the crafting menu after being held down 4 times (1.6 seconds)
    private void ToggleCrafting()
    {
        craftingMode = !craftingMode;
        if (Craftables.Count == 0) craftingMode = false;
        else if (!craftingMode && HeldCraftable == null && CanCraft(Craftables[craftingPage])) currentlyCrafting = Craftables[craftingPage];
    }

    // Finalizes crafting after being held down 6 times (2.4 seconds)
    private void FinalizeCrafting()
    {
        VentLogger.Trace($"{MyPlayer.GetNameWithRole()} => Finalize Crafting: {currentlyCrafting?.Name()}");
        HeldCraftable = currentlyCrafting;
        currentlyCrafting = null;
        HeldCraftable?.Ingredients().Entries().ForEach(i => heldIngredients[i.Key] -= i.Value);
        progressBar = "";
        waitForReleaseFlag = true;
    }

    [RoleAction(RoleActionType.OnPetRelease)]
    private void CancelCrafting()
    {
        if (advancePage) craftingPage++;
        if (craftingPage >= Craftables.Count) craftingPage = 0;
        VentLogger.Trace($"{MyPlayer.GetNameWithRole()} => Cancel Crafting");
        currentlyCrafting = null;
        progressBar = "";
        waitForReleaseFlag = false;
    }
}