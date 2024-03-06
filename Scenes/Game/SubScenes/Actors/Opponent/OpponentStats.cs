using Godot;
using System;
using Godot.Collections;

public partial class OpponentStats : ActorStats
{
    // Distance from center of ring when opponent starts strafing
    [Export] public float CenterStrafingDistance = 1;
    
    // Distance from target for when to stop closing in
    [Export] public float ClosingInDistance = 1;


    // dictionary containing all attacks using the attack type as the key
    public Dictionary<Opponent.AttackTypes, Array<string>> CurrentUniqueAttacks
        = new Dictionary<Opponent.AttackTypes, Array<string>>
        {
            { Opponent.AttackTypes.CloseToWall, new Array<string>() },
            { Opponent.AttackTypes.FarFromWall, new Array<string>() },
            { Opponent.AttackTypes.Neutral, new Array<string>() },
            { Opponent.AttackTypes.MixUp, new Array<string>() },
        };

    [Export] public Array<string> CloseToWallAttacks
    {
        get => CurrentUniqueAttacks[Opponent.AttackTypes.CloseToWall];
        set => CurrentUniqueAttacks[Opponent.AttackTypes.CloseToWall] = value;
    }
    [Export] public Array<string> FarFromWallAttacks
    {
        get => CurrentUniqueAttacks[Opponent.AttackTypes.FarFromWall];
        set => CurrentUniqueAttacks[Opponent.AttackTypes.FarFromWall] = value;
    }
    [Export] public Array<string> NeutralAttacks
    {
        get => CurrentUniqueAttacks[Opponent.AttackTypes.Neutral];
        set => CurrentUniqueAttacks[Opponent.AttackTypes.Neutral] = value;
    }
    [Export] public Array<string> MixUpAttacks
    {
        get => CurrentUniqueAttacks[Opponent.AttackTypes.MixUp];
        set => CurrentUniqueAttacks[Opponent.AttackTypes.MixUp] = value;
    }

    // how long to wait in idle before performing the next attack
    [Export] public float IdleDuration = 2f;
    [Export] public float CloseToWallDistance = 6f;
}
