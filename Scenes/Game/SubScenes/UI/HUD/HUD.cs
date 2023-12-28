using Godot;
using System;

public partial class HUD : Control
{
	[Export]
	Actor Player;

	[Export]
	Actor Opponent;

	ProgressBar PlayerHealth;
	ProgressBar OpponentHealth;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		PlayerHealth = GetNode<ProgressBar>("PlayerHealth");
		OpponentHealth = GetNode<ProgressBar>("OpponentHealth");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		PlayerHealth.Value = Player.HealthPercentage;
		OpponentHealth.Value = Opponent.HealthPercentage;
	}
}
