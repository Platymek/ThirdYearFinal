using Godot;
using System;

public partial class PlayerStats : ActorStats
{
    [Export] public float DodgeSpeedMultiplier = 3;
    [Export] public float DodgeDuration = .2f;
    [Export] public float DodgeStunDuration = .2f;
}
