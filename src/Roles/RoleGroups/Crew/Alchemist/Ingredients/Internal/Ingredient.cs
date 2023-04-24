using System;
using System.Collections.Generic;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace TOHTOR.Roles.RoleGroups.Crew.Ingredients;

[Localized("Roles.Alchemist.Ingredients")]
public abstract class Ingredient: IAlchemyIngredient
{
    public static List<IngredientInfo> AllIngredients = new();

    public static IngredientInfo Catalyst = new IngredientCatalyst().AsInfo();
    public static IngredientInfo Chaos = new IngredientChaos().AsInfo();
    public static IngredientInfo Death = new IngredientDeath(null!).AsInfo();
    public static IngredientInfo Shifting = new IngredientPurity(default).AsInfo();
    public static IngredientInfo Sight = new IngredientSight().AsInfo();
    public static IngredientInfo Tinkering = new IngredientTinkering(default).AsInfo();

    static Ingredient()
    {
        AllIngredients.Add(Catalyst);
        AllIngredients.Add(Chaos);
        AllIngredients.Add(Death);
        AllIngredients.Add(Shifting);
        AllIngredients.Add(Sight);
        AllIngredients.Add(Tinkering);
    }

    private DateTime spawnTime = DateTime.Now;
    private float lifespan;

    public Ingredient(float lifespan)
    {
        this.lifespan = lifespan;
    }

    public abstract string Name();

    public abstract Color Color();

    public abstract string Symbol();

    public abstract bool IsCollectable(Alchemist collector);

    public bool IsExpired() => DateTime.Now.Subtract(spawnTime).TotalSeconds > lifespan;

    public IngredientInfo AsInfo() => new(this);

    public virtual void Collect() {}
}

// Killing
// 1 Death, 1 Chaos, X Catalyst

// Shifting
// 2 Shifting, X Catalyst

// Increased Vision
// 1 Sight, X Catalyst

// Voting
// 2 Voting, X Catalyst

// Random TP
// 1 Chaos, X Catalyst