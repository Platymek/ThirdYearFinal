using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class AttackStats : Node
{
	[Export] public float Damage;
	[Export] public float Knock;
	[Export] public bool CheckDamage = false;
	[Export] public float TrackSpeedMultiplier = 1;
    [Export] public float DamageReactMutliplier = 1;
    [Export] public float KnockReactMutliplier = 1;
}
