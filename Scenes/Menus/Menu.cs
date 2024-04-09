using Godot;
using System;

public partial class Menu : Control
{
	// Properties //

	[Export] private Control initialButton;
	[Export] private Button _continueButton;

	public Global Global;


	// Node Functions //

	public override void _Ready()
	{
		base._Ready();

		// focus the specified button
		Focus();

        Global = GetNode<Global>("/root/Global");

		if (_continueButton != null)
        {
            _continueButton.Visible = Global.HasSave();
        }
	}


	// Signals //

	private void OnPlayPressed()
	{
        Global.PrepareNewGame();
        GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}

	private void OnMenuPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu/MainMenu.tscn");
	}

	private void OnSettingsPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/Settings/Settings.tscn");
	}

	private void OnResumePressed()
	{
		GetTree().Paused = false;
	}

	private void OnContinuePressed()
	{
		Global.PrepareContinueGame();
		GetTree().ChangeSceneToFile("res://Scenes/Menus/RoundNew/RoundNew.tscn");
	}


	// Other Functions //

	public void Focus()
	{
		initialButton.GrabFocus();
	}
}
