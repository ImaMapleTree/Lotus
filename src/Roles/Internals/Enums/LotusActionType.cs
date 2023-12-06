namespace Lotus.Roles.Internals.Enums;

public enum LotusActionType
{
    /// <summary>
    /// Represents no action
    /// </summary>
    None,
    /// <summary>
    /// Any action specifically taken by a player
    /// Parameters: (PlayerControl source, RoleAction action, object[] parameters)
    /// </summary>
    PlayerAction,
    /// <summary>
    /// Triggers when a player pets
    /// </summary>
    OnPet,
    /// <summary>
    /// Triggers when the pet button is held down. This gets sent every 0.4 seconds if the button is held down. The
    /// times parameter indicates how many times the button has been held down during the current span.
    /// <br/>
    /// Example: if times = 3 then the button has been held down for 1.2 seconds because 3 x 0.4 = 1.2
    /// </summary>
    /// <param name="times">the number of times the button has been detected in the down state (+1 every 0.4 seconds)</param>
    OnHoldPet,
    /// <summary>
    /// Triggers when the pet button has been held then released. Similar to <see cref="OnHoldPet"/>, the
    /// times parameter indicates how many times the button has been held down during the current span.
    /// </summary>
    /// <param name="times">the number of times the button has been detected in the down state (+1 every 0.4 seconds)</param>
    OnPetRelease,
    /// <summary>
    /// Triggers whenever the player enters a vent (this INCLUDES vent activation)
    /// Parameters: (Vent vent)
    /// </summary>
    VentEntered,
    VentExit,
    SuccessfulAngelProtect,
    SabotageStarted,
    /// <summary>
    /// Triggered when any one player fixes any part of a sabotage (I.E MiraHQ Comms) <br></br>
    /// Parameters: (SabotageType type, PlayerControl fixer, byte fixBit)
    /// </summary>
    SabotagePartialFix,
    SabotageFixed,
    Shapeshift,
    Unshapeshift,
    /// <summary>
    /// Triggered when my player attacks another player<br/>
    /// Parameters: (PlayerControl target)
    /// </summary>
    Attack,
    /// <summary>
    /// Triggers when a player dies. This cannot be canceled
    /// </summary>
    /// <param name="victim"><b>(GLOBAL ONLY)</b> <see cref="PlayerControl"/> the dead player</param>
    /// <param name="killer"><see cref="PlayerControl"/> the killing player</param>
    /// <param name="deathEvent"><see cref="Lotus.Managers.History.Events.IDeathEvent"/> the related death event </param>
    PlayerDeath,
    /// <summary>
    /// Triggers when any player gets exiled (by being voted out)
    /// </summary>
    /// <param name="victim"><b>(GLOBAL ONLY)</b> <see cref="PlayerControl"/> the exiled player</param>
    /// <param name="exiled"><see cref="GameData.PlayerInfo"/> the exiled player</param>
    Exiled,
    /// <summary>
    /// Triggers on Round Start (end of meetings, and start of game)
    /// Parameters: (bool isRoundOne)
    /// </summary>
    RoundStart,
    RoundEnd,
    /// <summary>
    /// Triggers when any player reports a body. <br></br>Parameters: (PlayerControl reporter, PlayerInfo reported)
    /// </summary>
    ReportBody,
    /// <summary>
    /// Triggers when any player completes a task. This cannot be canceled (Currently)
    /// </summary>
    /// <param name="player"><see cref="PlayerControl"/> the player completing the task</param>
    /// <param name="task"><see cref="Optional"/> an optional of <see cref="PlayerTask"/>, containing the task that was done</param>
    /// <param name="taskLength"><see cref="NormalPlayerTask.TaskLength"/> the length of the completed task</param>
    TaskComplete,
    FixedUpdate,
    /// <summary>
    /// Triggers when my player votes for someone (or skips)
    /// </summary>
    /// <param name="voter"><b>(GLOBAL ONLY)</b> <see cref="PlayerControl"/> the player voting</param>
    /// <param name="voted"><see cref="PlayerControl"/> the player voted for, or null if skipped</param>
    /// <param name="delegate"><see cref="MeetingDelegate"/> the meeting delegate for the current meeting</param>
    Vote,
    /// <summary>
    /// Triggers whenever another player interacts with THIS role
    /// </summary>
    /// <param name="target"><b>(GLOBAL ONLY)</b> <see cref="PlayerControl"/> the player being interacted with</param>
    /// <param name="interactor"><see cref="PlayerControl"/> the player starting the interaction</param>
    /// <param name="interaction"><see cref="Interaction"/> the interaction</param>
    Interaction,
    /// <summary>
    /// Triggers whenever a player sends a chat message. This action cannot be canceled.
    /// </summary>
    /// <param name="sender"><see cref="PlayerControl"/> the player who sent the chat message</param>
    /// <param name="message"><see cref="string"/> the message sent</param>
    /// <param name="state"><see cref="GameState"/> the current state of the game (for checking in meeting)</param>
    /// <param name="isAlive"><see cref="bool"/> if the chatting player is alive</param>
    Chat,
    /// <summary>
    /// Triggers whenever a player leaves the game. This action cannot be canceled
    /// </summary>
    /// <param name="player"><see cref="PlayerControl"/> the player who disconnected</param>
    Disconnect,
    /// <summary>
    /// Triggers when voting session ends. This action cannot be canceled.
    /// <b>IMPORTANT</b><br/>
    /// You CAN modify the meeting delegate at this time to change the results of the meeting. HOWEVER,
    /// modifying the votes will only change what is displayed during the meeting. You MUST also update the exiled player to change
    /// the exiled player, as the votes WILL NOT be recalculated automatically at this point. <see cref="MeetingDelegate.CalculateExiledPlayer"/>
    /// </summary>
    /// <param name="meetingDelegate"><see cref="MeetingDelegate"/> the meeting delegate for the current meeting</param>
    VotingComplete,
    /// <summary>
    /// Triggers when the meeting ends, this does not pass the meeting delegate as at this point everything has been finalized.
    /// <param name="Exiled Player">><see cref="Optional{T}"/> the optional exiled player</param>
    /// <param name="isTie"><see cref="bool"/> a boolean representing if the meeting tied</param>
    /// <param name="player vote counts"><see cref="Dictionary{TKey,TValue}"/> a dictionary containing (byte, int) representing the amount of votes a player got</param>
    /// <param name="playerVoteStatus"><see cref="Dictionary{TKey,TValue}"/> a dictionary containing (byte, List[Optional[byte]] containing the voting statuses of all players)</param>
    /// </summary>
    MeetingEnd,
    /// <summary>
    /// Triggers when a meeting is called
    /// </summary>
    /// <param name="player"><see cref="PlayerControl"/> the player who called the meeting</param>
    /// <param name="deadBody"><see cref="Optional{T}"/> optional <see cref="GameData.PlayerInfo"/> which exists if the meeting was called byt reporting a body</param>
    MeetingCalled
}