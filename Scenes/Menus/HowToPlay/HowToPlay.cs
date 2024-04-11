using Godot;
using System;

public partial class HowToPlay : Menu
{
	[Signal] public delegate void BackPressedEventHandler();

	private void OnBackPressed()
	{
		EmitSignal(SignalName.BackPressed);
	}

	private void OnBackPressedDefault()
	{
		Global.SaveFile.ConfirmHowToPlayRead();
		Global.ChangeScene(Global.Scene.MainMenu);
	}
}
