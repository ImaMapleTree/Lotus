using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.RoleGroups.Crew.Alchemist.Ingredients.Internal;

namespace Lotus.Roles.RoleGroups.Crew.Alchemist;

public partial class Alchemist
{
    private IAlchemyIngredient? collectableIngredient;

    [NewOnSetup] private Dictionary<IngredientInfo, int> heldIngredients;

    [RoleAction(RoleActionType.FixedUpdate)]
    private void CheckForIngredient()
    {
        if (DateTime.Now.Subtract(lastRun).TotalSeconds < AlchemistFixedUpdate) return;
        lastRun = DateTime.Now;

        GlobalIngredients.RemoveWhere(i => i.IsExpired());
        LocalIngredients.RemoveWhere(i => i.IsExpired());

        collectableIngredient =
            LocalIngredients.FirstOrDefault(i => i.IsCollectable(this))
            ?? GlobalIngredients.FirstOrDefault(i => i.IsCollectable(this));

        CheckChaosSpawn();
        CheckDiscussionSpawn();
    }

    [RoleAction(RoleActionType.OnPet)]
    private void CollectIngredient()
    {
        if (craftingMode) return;
        if (collectableIngredient == null) return;
        heldIngredients[collectableIngredient.AsInfo()] = heldIngredients.GetValueOrDefault(collectableIngredient.AsInfo(), 0) + 1;

        GlobalIngredients.Remove(collectableIngredient);
        LocalIngredients.Remove(collectableIngredient);
        collectableIngredient.Collect();
    }
}