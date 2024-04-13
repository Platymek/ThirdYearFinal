using Godot;
using System;

public partial class MainMenu : Menu
{
	[ExportGroup("Nodes")]
	[Export] private Button _continueButton;
	[Export] private Button _fullScreenButton;
	[Export] private Button _hardModeButton;
	[Export] private Button _playButton;
	[Export] private Button _howToPlayButton;


	// Signals //

	// needs to be called after the ready function because Global
	// may not have loaded the save file yet
	private void SetVisibilities()
	{
		_continueButton.Visible = Global.HasSave();
		_fullScreenButton.ButtonPressed = Global.SaveFile.Fullscreen;
		_playButton.Visible = Global.SaveFile.HowToPlayRead;

		// focus button based on what is and is not visible
		if (!_playButton.Visible)
		{
			_howToPlayButton.GrabFocus();
		}
		else if (!_continueButton.Visible)
		{
			_playButton.GrabFocus();
		}
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
