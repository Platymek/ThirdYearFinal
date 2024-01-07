using Godot;
using System;

public partial class Game : Node3D
{
	Menu _pause;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		_pause = GetNode<Menu>("HUD/Pause");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("pause"))
		{
			// toggle if the game is paused and apply that to the pause menu, focusing it
			// for use with gamepad
			GetTree().Paused = !GetTree().Paused;
			_pause.Visible = GetTree().Paused;
			_pause.Focus();
		}
	}
}
