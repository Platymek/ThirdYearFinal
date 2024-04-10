using Godot;
using System;

public partial class MainMenu : Menu
{
	[ExportGroup("Nodes")]
	[Export] private Button _continueButton;
	[Export] private Button _fullScreenButton;
	[Export] private Button _hardModeButton;


	// Node Functions //

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();


		// change appearance based on save data
		_continueButton.Visible = Global.HasSave();
		_hardModeButton.Visible = Global.SaveFile.UnlockedHardMode;
		_fullScreenButton.ButtonPressed = Global.SaveFile.Fullscreen;
	}


	// Signals //

	private void OnFullscreenToggled(bool toggled_on)
	{
		Global.Fullscreen = toggled_on;
	}

	private void OnStatsPressed()
	{
		Global.ChangeScene(Global.Scene.Stats);
	}
}
