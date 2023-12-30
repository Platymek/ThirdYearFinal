using Godot;
using System;
using Godot.Collections;

public partial class OpponentStats : ActorStats
{
    // Distance from center of ring when opponent starts strafing
    [Export] public float CenterStrafingDistance = 1;
    
    // Distance from target for when to stop closing in
    [Export] public float ClosingInDistance = 1;
    
    [Export] public Dictionary<Opponent.AttackTypes, Array<string>> CurrentUniqueAttacks;

    // how long to wait in idle before performing the next attack
    [Export] public float IdleDuration = 2f;
}
