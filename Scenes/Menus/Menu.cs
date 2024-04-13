using Godot;
using System;

public partial class Menu : Control
{
	// Properties //

	[Export] private Control _initialButton;

	public Global Global;


	// Node Functions //

	public override void _Ready()
	{
		base._Ready();

		// focus the specified button
		Focus();

		Global = GetNode<Global>("/root/Global");
	}


	// Signals //

	private void OnPlayPressed()
	{
		Global.PrepareNewGame();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");
		Global.ChangeScene(Global.Scene.Game);
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}

	private void OnMenuPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu/MainMenu.tscn");
        Global.ChangeScene(Global.Scene.MainMenu);
    }

	private void OnSettingsPressed()
	{
		// Settings not added yet
    }

	private void OnResumePressed()
	{
		GetTree().Paused = false;
	}

	private void OnContinuePressed(bool load)
	{
		if (load)
		{
			Global.PrepareContinueGame();
		}
		else
		{
			Global.OpponentStats.Delete();
		}

		GetTree().ChangeSceneToFile("res://Scenes/Menus/RoundNew/RoundNew.tscn");
        Global.ChangeScene(Global.Scene.RoundNew);
    }


	// Other Functions //

	public void Focus()
	{
		_initialButton.GrabFocus();
	}
}
