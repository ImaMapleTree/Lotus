using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TOHTOR.Roles;
using TOHTOR.Roles.Debugger;
using TOHTOR.Roles.Extra;
using TOHTOR.Roles.RoleGroups.Coven;
using TOHTOR.Roles.RoleGroups.Crew;
using TOHTOR.Roles.RoleGroups.Crew.Snitch;
using TOHTOR.Roles.RoleGroups.Impostors;
using TOHTOR.Roles.RoleGroups.Madmates.Roles;
using TOHTOR.Roles.RoleGroups.Neutral;
using TOHTOR.Roles.RoleGroups.NeutralKilling;
using TOHTOR.Roles.RoleGroups.Undead.Roles;
using TOHTOR.Roles.RoleGroups.Vanilla;
using TOHTOR.Roles.Subroles;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Extensions;
using static TOHTOR.Roles.AbstractBaseRole;
using Impostor = TOHTOR.Roles.RoleGroups.Vanilla.Impostor;
using Medium = TOHTOR.Roles.RoleGroups.Crew.Medium;
using Necromancer = TOHTOR.Roles.RoleGroups.Undead.Roles.Necromancer;
using SerialKiller = TOHTOR.Roles.RoleGroups.Impostors.SerialKiller;

namespace TOHTOR.Managers;

[LoadStatic]
public static class CustomRoleManager
{
    public static Dictionary<byte, CustomRole> PlayersCustomRolesRedux = new();
    public static Dictionary<byte, CustomRole> LastRoundCustomRoles = new();

    public static Dictionary<byte, List<CustomRole>> PlayerSubroles = new();

    public static List<byte> RoleBlockedPlayers = new();
    public static StaticRoles Static = new();
    public static ExtraRoles Special = new();
    public static CustomRole Default = Static.Crewmate;

    public static readonly List<CustomRole> MainRoles = Static.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Static))
        .ToList();

    public static readonly List<CustomRole> SpecialRoles = Special.GetType()
        .GetFields()
        .Select(f => (CustomRole)f.GetValue(Special))
        .ToList();

    public static readonly List<CustomRole> AllRoles = MainRoles.Concat(SpecialRoles).ToList();


    public static void AddRole(CustomRole staticRole)
    {
        AllRoles.Add(staticRole);
    }

    public static int GetRoleId(CustomRole role) => role == null ? 0 : GetRoleId(role.GetType());
    public static CustomRole GetRoleFromType(Type roleType) => GetRoleFromId(GetRoleId(roleType));

    public static int GetRoleId(Type roleType)
    {
        for (int i = 0; i < AllRoles.Count; i++)
            if (roleType == AllRoles[i].GetType())
                return i;
        return -1;
    }

    public static CustomRole GetRoleFromId(int id)
    {
        if (id == -1) id = 0;
        return AllRoles[id];
    }

    public static void AddPlayerSubrole(byte playerId, Subrole subrole)
    {
        if (!PlayerSubroles.ContainsKey(playerId)) PlayerSubroles[playerId] = new List<CustomRole>();
        PlayerSubroles[playerId].Add(subrole);
    }

    public static void AddPlayerSubrole(byte playerId, CustomRole subrole)
    {
        if (!PlayerSubroles.ContainsKey(playerId)) PlayerSubroles[playerId] = new List<CustomRole>();
        PlayerSubroles[playerId].Add(subrole);
    }

    internal static void LinkEditor(Type editorType)
    {
        if (!editorType.IsAssignableTo(typeof(RoleEditor)))
            throw new ArgumentException("Editor Type MUST be a subclass of AbstractBaseRole.RoleEditor");
        Type roleType = editorType.BaseType!.DeclaringType!;
        bool isStatic = typeof(StaticRoles).GetFields().Any(f => f.FieldType == roleType);
        bool isExtra = typeof(ExtraRoles).GetFields().Any(f => f.FieldType == roleType);

        CustomRole role = GetRoleFromType(roleType);
        ConstructorInfo editorCtor = editorType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { roleType })!;
        RoleEditor editor = (RoleEditor)editorCtor.Invoke(new object?[] {role});
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
        if (!editorType.IsAssignableTo(typeof(RoleEditor)))
            throw new ArgumentException("Editor Type MUST be a subclass of AbstractBaseRole.RoleEditor");
        Type roleType = editorType.BaseType!.DeclaringType!;
        bool isStatic = typeof(StaticRoles).GetFields().Any(f => f.FieldType == roleType);
        bool isExtra = typeof(ExtraRoles).GetFields().Any(f => f.FieldType == roleType);

        CustomRole role = GetRoleFromType(roleType);
        RoleEditor editor = role.Editor;

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
        public BountyHunter BountyHunter = new BountyHunter();
        public Camouflager Camouflager = new Camouflager();
        //cleaner
        public Consort Consort = new Consort();
        public Disperser Disperser = new Disperser();
        //escapist
        public FireWorks FireWorks = new FireWorks();
        public Freezer Freezer = new Freezer();
        public Grenadier Grenadier = new Grenadier();
        //identity thief
        public Impostor Impostor = new Impostor();
        public Janitor Janitor = new Janitor();
        public LastImpostor LastImpostor = new LastImpostor();
        public Madmate Madmate = new Madmate();
        //mad guardian
        //mad snitch
        public Mafia Mafia = new Mafia();
        //manipulator
        public Mare Mare = new Mare();
        public Miner Miner = new Miner();
        public Morphling Morphling = new Morphling();
        public Ninja Ninja = new Ninja();
        //parasite
        //pickpocket
        public Puppeteer Puppeteer = new Puppeteer();
        //sidekick madmate
        //silencer
        public Sniper Sniper = new Sniper();
        public Swooper Swooper = new Swooper();
        public TimeThief TimeThief = new TimeThief();
        //traitor
        public Vampire Vampire = new Vampire();
        public Vampiress Vampiress = new Vampiress();
        public Warlock Warlock = new Warlock();
        public Witch Witch = new Witch();
        public YingYanger YingYanger = new YingYanger();


        //Crewmates


        public Baiter Baiter = new Baiter();
        public Bastion Bastion = new Bastion();
        public Bodyguard Bodyguard = new Bodyguard();
        public Child Child = new Child();
        public Crewmate Crewmate = new Crewmate();
        public Crusader Crusader = new Crusader();
        public Demolitionist Demolitionist = new Demolitionist();
        //dictator
        //doctor
        public Engineer Engineer = new Engineer();
        //escort
        public Investigator Investigator = new Investigator();
        public Mayor Mayor = new Mayor();
        //mechanic
        public Medium Medium = new Medium();
        public Mystic Mystic = new Mystic();
        public Observer Observer = new Observer();
        public Oracle Oracle = new Oracle();
        //physicist
        //psychic
        //sabotage master
        public Scientist Scientist = new Scientist();
        public Sheriff Sheriff = new Sheriff();
        public Snitch Snitch = new Snitch();
        public Speedrunner Speedrunner = new Speedrunner();
        public Transporter Transporter = new Transporter();
        public Trapper Trapper = new Trapper();
        public Vigilante Vigilante = new Vigilante();
        public Veteran Veteran = new Veteran();


        //Neutrals


        //agitater
        public Amnesiac Amnesiac = new Amnesiac();
        public Archangel Archangel = new Archangel();
        public Arsonist Arsonist = new Arsonist();
        public BloodKnight BloodKnight = new BloodKnight();
        public CrewPostor CrewPostor = new CrewPostor();
        public Egoist Egoist = new Egoist();
        public Executioner Executioner = new Executioner();
        public Glitch Glitch = new Glitch();
        public GuardianAngel GuardianAngel = new GuardianAngel();
        //hacker
        //hitman
        public Jackal Jackal = new Jackal();
        public Jester Jester = new Jester();
        //juggernaught
        //marksman
        //neutral witch
        public Opportunist Opportunist = new Opportunist();
        public Pestilence Pestilence = new Pestilence();
        public Phantom Phantom = new Phantom();
        public PlagueBearer PlagueBearer = new PlagueBearer();
        //postman
        public SchrodingerCat SchrodingerCat = new SchrodingerCat();
        public SerialKiller SerialKiller = new SerialKiller();
        public Sidekick Sidekick = new Sidekick();
        public Survivor Survivor = new Survivor();
        //swapper
        public Terrorist Terrorist = new Terrorist();
        //vulture
        //werewolf
    }

    public class ExtraRoles
    {
        public GM GM = new GM();
        public Bait Bait = new Bait();
        public Bewilder Bewilder = new Bewilder();
        public Coven Coven = new Coven();
        public Debugger Debugger = new Debugger();
        public Diseased Diseased = new Diseased();
        //double shot
        //flash
        public Fox Fox = new();
        public Lovers Lovers = new Lovers();
        //oblivious
        //obvious
        //sleuth
        //torch
        //watcher
    }
}
