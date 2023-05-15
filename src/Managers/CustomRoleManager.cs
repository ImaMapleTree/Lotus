using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Debugger;
using Lotus.Roles.Extra;
using Lotus.Roles.Internals;
using Lotus.Roles.RoleGroups.Coven;
using Lotus.Roles.RoleGroups.Crew;
using Lotus.Roles.RoleGroups.Impostors;
using Lotus.Roles.RoleGroups.Madmates.Roles;
using Lotus.Roles.RoleGroups.Neutral;
using Lotus.Roles.RoleGroups.NeutralKilling;
using Lotus.Roles.RoleGroups.Undead.Roles;
using Lotus.Roles.RoleGroups.Vanilla;
using Lotus.Roles.Subroles;
using Lotus.Options.Roles;
using VentLib.Options;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using static Lotus.Roles.AbstractBaseRole;
using Impostor = Lotus.Roles.RoleGroups.Vanilla.Impostor;
using Medium = Lotus.Roles.RoleGroups.Crew.Medium;
using Necromancer = Lotus.Roles.RoleGroups.Undead.Roles.Necromancer;
using SerialKiller = Lotus.Roles.RoleGroups.Impostors.SerialKiller;

namespace Lotus.Managers;

[LoadStatic]
public static class CustomRoleManager
{
    public static OptionManager RoleOptionManager = OptionManager.GetManager(file: "role_options.txt");
    /*public static Dictionary<byte, CustomRole> PlayerRoles = new();
    public static Dictionary<byte, List<CustomRole>> PlayerSubroles = new();*/

    public static StaticRoles Static = new();
    public static Modifiers Mods = new();
    public static ExtraRoles Special = new();
    public static CustomRole Default = Static.Crewmate;

    private static List<CustomRole>? _lazyInitializeList;

    public static readonly List<CustomRole> MainRoles = Static.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Static)!)
        .ToList();
    
    public static readonly List<CustomRole> ModifierRoles = Mods.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Mods)!)
        .ToList();

    public static readonly List<CustomRole> SpecialRoles = Special.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Special)!)
        .ToList();

    public static readonly List<CustomRole> AllRoles = MainRoles.Concat(SpecialRoles).Concat(ModifierRoles).Concat(_lazyInitializeList!).ToList();

    public static void AddRole(CustomRole staticRole)
    {
        if (AllRoles == null!) (_lazyInitializeList ??= new List<CustomRole>()).Add(staticRole);
        else AllRoles.Add(staticRole);
    }

    public static int GetRoleId(CustomRole role) => role == null ? 0 : GetRoleId(role.GetType());
    public static CustomRole GetRoleFromType(Type roleType) => GetRoleFromId(GetRoleId(roleType));
    public static CustomRole GetCleanRole(CustomRole role) => GetRoleFromId(GetRoleId(role));

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

        //assassin
        //bomber
        public Blackmailer Blackmailer = new Blackmailer();
        public BountyHunter BountyHunter = new BountyHunter();
        public Camouflager Camouflager = new Camouflager();
        public Consort Consort = new Consort();
        public Creeper Creeper = new Creeper();
        public Disperser Disperser = new Disperser();
        public Escapist Escapist = new Escapist();
        public FireWorker FireWorker = new FireWorker();
        public Freezer Freezer = new Freezer();
        public Grenadier Grenadier = new Grenadier();
        public IdentityThief IdentityThief = new IdentityThief();
        public Roles.RoleGroups.Vanilla.Impostor Impostor = new Roles.RoleGroups.Vanilla.Impostor();
        public Janitor Janitor = new Janitor();
        public Mafioso Mafioso = new Mafioso();
        public Mastermind Mastermind = new Mastermind();
        public Conman Conman = new Conman();
        public Mare Mare = new Mare();
        public Miner Miner = new Miner();
        public Morphling Morphling = new Morphling();
        public Ninja Ninja = new Ninja();
        public PickPocket PickPocket = new PickPocket();
        public Puppeteer Puppeteer = new Puppeteer();
        //sidekick madmate
        //silencer
        public Roles.RoleGroups.Impostors.SerialKiller SerialKiller = new Roles.RoleGroups.Impostors.SerialKiller();
        public Sniper Sniper = new Sniper();
        public Swooper Swooper = new Swooper();
        public TimeThief TimeThief = new TimeThief();
        //traitor
        public Vampire Vampire = new Vampire();
        public Warlock Warlock = new Warlock();
        public Witch Witch = new Witch();
        public YinYanger YinYanger = new YinYanger();

        public CrewPostor CrewPostor = new CrewPostor();
        public Madmate Madmate = new Madmate();
        public MadGuardian MadGuardian = new MadGuardian();
        public MadSnitch MadSnitch = new MadSnitch();
        public Parasite Parasite = new Parasite();

        //Crewmates

        public Alchemist Alchemist = new Alchemist();
        public Bastion Bastion = new Bastion();
        public Bodyguard Bodyguard = new Bodyguard();
        public Crewmate Crewmate = new Crewmate();
        public Crusader Crusader = new Crusader();
        public Demolitionist Demolitionist = new Demolitionist();
        public Dictator Dictator = new Dictator();
        public Doctor Doctor = new Doctor();

        public Escort Escort = new Escort();
        public ExConvict ExConvict = new ExConvict();
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
        public SabotageMaster SabotageMaster = new SabotageMaster();
        public Sheriff Sheriff = new Sheriff();
        public Snitch Snitch = new Snitch();
        public Speedrunner Speedrunner = new Speedrunner();
        public Swapper Swapper = new Swapper();
        public Transporter Transporter = new Transporter();
        public Trapper Trapper = new Trapper();
        public Vigilante Vigilante = new Vigilante();
        public Veteran Veteran = new Veteran();


        //Neutrals

        // ReSharper disable once InconsistentNaming
        private CustomRole LOAD_NEUTRAL_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.LoadNeutralOptions());

        public AgiTater AgiTater = new AgiTater();
        public Amnesiac Amnesiac = new Amnesiac();
        public Archangel Archangel = new Archangel();
        public Arsonist Arsonist = new Arsonist();
        public BloodKnight BloodKnight = new BloodKnight();
        public Copycat Copycat = new Copycat();
        public Egoist Egoist = new Egoist();
        public Executioner Executioner = new Executioner();
        public Glitch Glitch = new Glitch();
        public GuardianAngel GuardianAngel = new GuardianAngel();
        public Hacker Hacker = new Hacker();
        public Hitman Hitman = new Hitman();
        public Jackal Jackal = new Jackal();
        public Jester Jester = new Jester();
        public Juggernaut Juggernaut = new Juggernaut();
        public Marksman Marksman = new Marksman();
        public Roles.RoleGroups.Undead.Roles.Necromancer Necromancer = new Roles.RoleGroups.Undead.Roles.Necromancer();
        //neutral witch
        public Opportunist Opportunist = new Opportunist();
        public Phantom Phantom = new Phantom();
        public PlagueBearer PlagueBearer = new PlagueBearer();
        public Pestilence Pestilence = new Pestilence();
        public Postman Postman = new Postman();
        public Retributionist Retributionist = new Retributionist();
        public Sidekick Sidekick = new Sidekick();
        public Survivor Survivor = new Survivor();
        public Terrorist Terrorist = new Terrorist();
        public Vulture Vulture = new Vulture();
        public Werewolf Werewolf = new Werewolf();

        public CustomRole LOAD_MODIFIER_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.LoadSubroleOptions());
    }

    public class Modifiers
    {
        public Bait Bait = new Bait();
        public Bewilder Bewilder = new Bewilder();
        public Diseased Diseased = new Diseased();
        public Flash Flash = new Flash();
        public Oblivious Oblivious = new Oblivious();
        public Psychopath Psychopath = new Psychopath();
        public Sleuth Sleuth = new Sleuth();
        public Torch Torch = new Torch();
    }

    public class ExtraRoles
    {
        public IllegalRole IllegalRole = new IllegalRole();

        public GM GM = new GM();
        public Coven Coven = new Coven();
        public Debugger Debugger = new Debugger();
        
        //double shot
        //flash
        public Fox Fox = new();
        public LoversReal LoversReal = new LoversReal();
        //oblivious
        //obvious
        //sleuth
        //torch
        //watcher
    }
}
