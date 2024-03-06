using Godot;
using System;

[Tool]
public partial class OpponentAttackStats : AttackStats
{
	[Export] public float ClosingInSpeed = 1;
	[Export] public float StrafingSpeed = .25f;
}
