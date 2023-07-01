/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus.Logging;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Debugger;
using Lotus.Roles.Extra;
using Lotus.Roles.Internals;
using Lotus.Roles.RoleGroups.Crew;
using Lotus.Roles.RoleGroups.Crew.Alchemist;
using Lotus.Roles.RoleGroups.Impostors;
using Lotus.Roles.RoleGroups.Madmates.Roles;
using Lotus.Roles.RoleGroups.Neutral;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Roles.RoleGroups.Stock;
using Lotus.Roles.RoleGroups.Undead.Roles;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Roles.Subroles;
using Lotus.Roles.Subroles.Guessers;
using Lotus.Roles.Subroles.Romantics;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using Medium = Lotus.Roles.RoleGroups.Crew.Medium;
using Pirate = Lotus.Roles.RoleGroups.Neutral.Pirate;

namespace Lotus.Managers;

[LoadStatic]
public static class CustomRoleManager
{
    public static OptionManager RoleOptionManager = OptionManager.GetManager(file: "role_options.txt");

    public static StaticRoles Static;
    public static Modifiers Mods;
    public static ExtraRoles Special;
    public static CustomRole Default;

    public static readonly List<CustomRole> MainRoles;

    public static readonly List<CustomRole> ModifierRoles;

    public static readonly List<CustomRole> SpecialRoles;

    public static List<CustomRole> AllRoles;

    static CustomRoleManager()
    {
        AllRoles = new List<CustomRole>();
        Static = new StaticRoles();
        Mods = new Modifiers();
        Special = new ExtraRoles();
        Default = Special.EmptyRole;

        MainRoles = Static.GetType()
            .GetFields()
            .Select(f => (CustomRole)f.GetValue(Static)!)
            .ToList();

        ModifierRoles = Mods.GetType()
            .GetFields()
            .Select(f => (CustomRole)f.GetValue(Mods)!)
            .ToList();

        SpecialRoles = Special.GetType()
            .GetFields()
            .Select(f => (CustomRole)f.GetValue(Special)!)
            .ToList();

        List<CustomRole> realAllRoleList = MainRoles;
        realAllRoleList.AddRange(ModifierRoles);
        realAllRoleList.AddRange(SpecialRoles);
        realAllRoleList.AddRange(AllRoles);
        AllRoles = realAllRoleList;
        AllRoles.ForEach(r => r.Solidify());
    }


    public static void AddRole(CustomRole staticRole)
    {
        AllRoles.Add(staticRole);
    }

    public static int GetRoleId(CustomRole role) => role == null ? 0 : GetRoleId(role.GetType());
    public static CustomRole GetRoleFromType(Type roleType) => GetRoleFromId(GetRoleId(roleType));
    public static CustomRole GetCleanRole(CustomRole role) => GetRoleFromId(GetRoleId(role));

    public static CustomRole RoleFromQualifier(string qualifier)
    {
        return AllRoles.FirstOrDefault(r => QualifierFromRole(r) == qualifier, Default);
    }

    public static string QualifierFromRole(CustomRole role)
    {
        return $"{role.DeclaringAssembly.GetName().Name ?? "Unknown"}.{role.GetType().Name}.{role.EnglishRoleName}";
    }

    public static int GetRoleId(Type roleType)
    {
        for (int i = 0; i < AllRoles.Count; i++)
            if (roleType == AllRoles[i].GetType())
                return i;
        return -1;
    }

    public static CustomRole GetRoleFromName(string name) => AllRoles.First(r => r.EnglishRoleName == name);

    public static CustomRole GetRoleFromId(int id)
    {
        if (id == -1) id = 0;
        return AllRoles[id];
    }

    internal static void LinkEditor(Type editorType)
    {
        if (!editorType.IsAssignableTo(typeof(AbstractBaseRole.RoleEditor)))
            throw new ArgumentException("Editor Type MUST be a subclass of AbstractBaseRole.RoleEditor");
        Type roleType = editorType.BaseType!.DeclaringType!;
        bool isStatic = typeof(StaticRoles).GetFields().Any(f => f.FieldType == roleType);
        bool isExtra = typeof(ExtraRoles).GetFields().Any(f => f.FieldType == roleType);

        CustomRole role = GetRoleFromType(roleType);
        ConstructorInfo editorCtor = editorType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { roleType })!;
        AbstractBaseRole.RoleEditor editor = (AbstractBaseRole.RoleEditor)editorCtor.Invoke(new object?[] {role});
        CustomRole modified = (CustomRole)editor.StartLink();

        if (isStatic) {
            typeof(StaticRoles).GetField(roleType.Name)?.SetValue(Static, modified);
            MainRoles.Replace(role, modified);
        }

        if (isExtra) {
            typeof(ExtraRoles).GetField(roleType.Name)?.SetValue(Special, modified);
            SpecialRoles.Replace(role, modified);
        }

        AllRoles.Replace(role, modified);
    }

    internal static void RemoveEditor(Type editorType)
    {
        if (!editorType.IsAssignableTo(typeof(AbstractBaseRole.RoleEditor)))
            throw new ArgumentException("Editor Type MUST be a subclass of AbstractBaseRole.RoleEditor");
        Type roleType = editorType.BaseType!.DeclaringType!;
        bool isStatic = typeof(StaticRoles).GetFields().Any(f => f.FieldType == roleType);
        bool isExtra = typeof(ExtraRoles).GetFields().Any(f => f.FieldType == roleType);

        CustomRole role = GetRoleFromType(roleType);
        AbstractBaseRole.RoleEditor editor = role.Editor;

        if (isStatic) {
            typeof(StaticRoles).GetField(roleType.Name)?.SetValue(Static, editor.FrozenRole);
            MainRoles.Replace(role, (CustomRole)editor.FrozenRole);
        }

        if (isExtra) {
            typeof(ExtraRoles).GetField(roleType.Name)?.SetValue(Special, editor.FrozenRole);
            SpecialRoles.Replace(role, (CustomRole)editor.FrozenRole);
        }

        AllRoles.Replace(role, (CustomRole)editor.FrozenRole);
    }

    public class StaticRoles
    {
        //Impostors

        public CustomRole LOAD_IMPOSTOR_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.LoadImpostorOptions());
        //assassin
        //bomber
        public Assassin Assassin = new Assassin();
        public Blackmailer Blackmailer = new Blackmailer();
        public BountyHunter BountyHunter = new BountyHunter();
        public Camouflager Camouflager = new Camouflager();
        public Consort Consort = new Consort();
        public Creeper Creeper = new Creeper();
        public Disperser Disperser = new Disperser();
        public Escapist Escapist = new Escapist();
        /*public FireWorks FireWorks = new FireWorks();#1#
        public Freezer Freezer = new Freezer();
        public Grenadier Grenadier = new Grenadier();
        public IdentityThief IdentityThief = new IdentityThief();
        public Impostor Impostor = new Impostor();
        public Janitor Janitor = new Janitor();
        public Mafioso Mafioso = new Mafioso();
        public Mare Mare = new Mare();
        public Mastermind Mastermind = new Mastermind();
        public Miner Miner = new Miner();
        public Morphling Morphling = new Morphling();
        public Ninja Ninja = new Ninja();
        public PickPocket PickPocket = new PickPocket();
        public Puppeteer Puppeteer = new Puppeteer();
        //sidekick madmate
        //silencer
        public SerialKiller SerialKiller = new SerialKiller();
        public Sniper Sniper = new Sniper();
        public Swooper Swooper = new Swooper();
        public TimeThief TimeThief = new TimeThief();
        //traitor
        public Vampire Vampire = new Vampire();
        public Warlock Warlock = new Warlock();
        public Witch Witch = new Witch();
        public YinYanger YinYanger = new YinYanger();

        public CustomRole MADMATE_TITLE = new EnforceFunctionOrderingRole(() => RoleOptions.LoadMadmateOptions());

        public CrewPostor CrewPostor = new CrewPostor();
        public Madmate Madmate = new Madmate();
        public MadGuardian MadGuardian = new MadGuardian();
        public MadSnitch MadSnitch = new MadSnitch();
        public Parasite Parasite = new Parasite();

        //Crewmates
        public CustomRole LOAD_CREW_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.LoadCrewmateOptions());

        public Alchemist Alchemist = new Alchemist();
        public Bastion Bastion = new Bastion();
        public Bodyguard Bodyguard = new Bodyguard();
        public Chameleon Chameleon = new Chameleon();
        public Charmer Charmer = new Charmer();
        public Crewmate Crewmate = new Crewmate();
        public Crusader Crusader = new Crusader();
        public Demolitionist Demolitionist = new Demolitionist();
        public Dictator Dictator = new Dictator();
        public Doctor Doctor = new Doctor();

        public Escort Escort = new Escort();
        public ExConvict ExConvict = new ExConvict();
        public Herbalist Herbalist = new Herbalist();
        public Investigator Investigator = new Investigator();
        public Mayor Mayor = new Mayor();
        public Mechanic Mechanic = new Mechanic();
        public Medic Medic = new Medic();
        public Medium Medium = new Roles.RoleGroups.Crew.Medium();
        public Mystic Mystic = new Mystic();
        public Observer Observer = new Observer();
        public Oracle Oracle = new Oracle();
        public Physicist Physicist = new Physicist();
        public Psychic Psychic = new Psychic();
        public Repairman Repairman = new Repairman();
        public Sheriff Sheriff = new Sheriff();
        public Snitch Snitch = new Snitch();
        public Speedrunner Speedrunner = new Speedrunner();
        public Swapper Swapper = new Swapper();
        public Tracker Tracker = new Tracker();
        public Transporter Transporter = new Transporter();
        public Trapster Trapster = new Trapster();
        public Veteran Veteran = new Veteran();
        public Vigilante Vigilante = new Vigilante();


        //Neutrals

        // ReSharper disable once InconsistentNaming
        public CustomRole LOAD_NEUTRAL_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.LoadNeutralOptions());

        public CustomRole NEUTRAL_KILLING_TITLE = new EnforceFunctionOrderingRole(() => new GameOptionTitleBuilder().Title("<size=2.3>★ Neutral Killing ★</size>").Color(ModConstants.Palette.KillingColor).Tab(DefaultTabs.NeutralTab).Build());

        public AgiTater AgiTater = new AgiTater();
        public Arsonist Arsonist = new Arsonist();
        public BloodKnight BloodKnight = new BloodKnight();
        public Demon Demon = new Demon();
        public Egoist Egoist = new Egoist();
        public Hitman Hitman = new Hitman();
        public Jackal Jackal = new Jackal();
        public Juggernaut Juggernaut = new Juggernaut();
        public Marksman Marksman = new Marksman();
        public Necromancer Necromancer = new Necromancer();
        public Occultist Occultist = new Occultist();
        public Pelican Pelican = new Pelican();
        public PlagueBearer PlagueBearer = new PlagueBearer();
        public Retributionist Retributionist = new Retributionist();
        public Glitch Glitch = new Glitch();
        public Werewolf Werewolf = new Werewolf();

        public CustomRole NEUTRAL_PASSIVE_TITLE = new EnforceFunctionOrderingRole(() => new GameOptionTitleBuilder().Title("<size=2.3>❀ Neutral Passive ❀</size>").Color(ModConstants.Palette.PassiveColor).Tab(DefaultTabs.NeutralTab).Build());

        public Amnesiac Amnesiac = new Amnesiac();
        /*public Archangel Archangel = new Archangel();#1#
        public Copycat Copycat = new Copycat();
        public Executioner Executioner = new Executioner();
        public Hacker Hacker = new Hacker();
        public Jester Jester = new Jester();
        public Opportunist Opportunist = new Opportunist();
        public Phantom Phantom = new Phantom();
        public Pirate Pirate = new Pirate();
        public Postman Postman = new Postman();
        public SchrodingersCat SchrodingersCat = new SchrodingersCat();
        public Survivor Survivor = new Survivor();
        public Terrorist Terrorist = new Terrorist();
        public Vulture Vulture = new Vulture();

        /*public Guesser Guesser = new Guesser();#1#
        public CustomRole LOAD_MODIFIER_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.LoadSubroleOptions());
    }

    public class Modifiers
    {
        public Bait Bait = new Bait();
        public Bewilder Bewilder = new Bewilder();
        public Bloodlust Bloodlust = new Bloodlust();
        public Deadly Deadly = new Deadly();
        public Diseased Diseased = new Diseased();
        public Flash Flash = new Flash();
        public Honed Honed = new Honed();
        public Nimble Nimble = new Nimble();
        public Oblivious Oblivious = new Oblivious();
        public Romantic Romantic = new Romantic();
        public Sleuth Sleuth = new Sleuth();
        public TieBreaker TieBreaker = new TieBreaker();
        public Torch Torch = new Torch();
        public Unstoppable Unstoppable = new Unstoppable();
        public Watcher Watcher = new Watcher();
        public Workhorse Workhorse = new Workhorse();
    }

    public class ExtraRoles
    {
        public IllegalRole IllegalRole = new IllegalRole();
        public GM GM = new GM();
        public Debugger Debugger = new Debugger();
        public EmptyRole EmptyRole = new EmptyRole();

        public CrewGuesser CrewGuesser = new CrewGuesser();
        public ImpGuesser ImpGuesser = new ImpGuesser();
        public NeutralKillerGuesser NeutralKillerGuesser = new NeutralKillerGuesser();
        public NeutralGuesser NeutralGuesser = new NeutralGuesser();

        //double shot
        //flash
        //oblivious
        //obvious
        //sleuth
        //torch
        //watcher
    }
}
*/
