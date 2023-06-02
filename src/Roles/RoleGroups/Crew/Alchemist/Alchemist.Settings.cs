using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.Roles.RoleGroups.Crew.Potions;
using Lotus.Extensions;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace Lotus.Roles.RoleGroups.Crew;

public partial class Alchemist
{
    public static readonly List<Func<int, Potion>> PotionConstructors = new()
    {
        i => new PotionKilling(i), i => new PotionProtection(i), i => new PotionSeeing(i),
        i => new PotionTeleportation(i), i => new PotionSabotage(i), i => new PotionVoting(i),
        i => new PotionRevealing(i), i => new PotionRandom(i)
    };

    private HashSet<Type> bannedPotions = new();
    private int baseCatalystAmount;
    private bool modifyPotionSettings;
    public List<ICraftable> Craftables = new();


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream)
    {
        var builder = base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Base Catalyst Amount")
                .AddIntRange(0, 8, 1, 2)
                .BindInt(i => baseCatalystAmount = i)
                .Build());
        PotionConstructors.Select(p => p(0)).ForEach(p =>
        {
            builder = builder.SubOption(sub => sub
                .Name(p.Color().Colorize(p.Name()))
                .Key(p.GetType().Name)
                .AddOnOffValues()
                .BindBool(b =>
                {
                    if (b) bannedPotions.Remove(p.GetType());
                    else bannedPotions.Add(p.GetType());
                }).Build()
            );
        });
        return builder;
    }
}