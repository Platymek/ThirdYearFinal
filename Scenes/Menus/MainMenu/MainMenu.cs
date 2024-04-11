using Godot;
using System;

public partial class MainMenu : Menu
{
	[ExportGroup("Nodes")]
	[Export] private Button _continueButton;
	[Export] private Button _fullScreenButton;
	[Export] private Button _hardModeButton;
	[Export] private Button _playButton;


	// Signals //

	// needs to be called after the ready function because Global
	// may not have loaded the save file yet
	private void SetVisibilities()
	{
		_fullScreenButton.ButtonPressed = Global.SaveFile.Fullscreen;
		_continueButton.Visible = Global.HasSave();
		_hardModeButton.Visible = Global.SaveFile.UnlockedHardMode;
		_playButton.Visible = Global.SaveFile.HowToPlayRead;
	}

	private void OnFullscreenToggled(bool toggled_on)
	{
		Global.Fullscreen = toggled_on;
	}

	private void OnStatsPressed()
	{
		Global.ChangeScene(Global.Scene.Stats);
	}

	private void OnHowToPlayPressed()
	{
		Global.ChangeScene(Global.Scene.HowToPlay);
	}
}
