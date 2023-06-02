using System;

namespace Lotus.Gamemodes;

[Flags]
public enum GameAction
{
    ReportBody = 1,
    CallMeeting = 2,
    KillPlayers = 4,
    CallSabotage = 8,
    CloseDoors = 16,
    EnterVent = 32,

    // These flags cannot be blocked so it doesn't matter if we set them to the following
    GameJoin,
    GameLeave,
    GameStart,
}