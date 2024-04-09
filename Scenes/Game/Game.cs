using Godot;
using System;

public partial class Game : Node3D
{
	// Properties //

	[Export] private Timer _timer;
	private Menu _pause;


	// Node Functions //

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


	// Signals //

	private void WinGame()
	{
		GetNode<Global>("/root/Global").CurrentRoundProgress++;
        GetNode<Global>("/root/Global").EndRoundWin();
		_timer.Stop();
	}

	private void LoseGame()
	{
		GetNode<Global>("/root/Global").EndRoundLoss();
		_timer.Stop();
	}
}
