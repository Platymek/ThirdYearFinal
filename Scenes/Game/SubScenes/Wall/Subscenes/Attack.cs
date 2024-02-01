using Godot;
using System;

public partial class Attack : Node
{
	[Export] public string Description;
	[Export] public Opponent.AttackTypes AttackType;
}
