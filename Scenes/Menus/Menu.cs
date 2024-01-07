using Godot;
using System;

public partial class Menu : Control
{
	// Properties //

	private Control _buttons;


	// Node Functions //

	public override void _Ready()
	{
		base._Ready();

		// focus the first button in the menu so that a controller can be used
		_buttons = GetNode<Control>("AspectRatioContainer/CenterContainer/VBoxContainer/Container/Buttons");
		Focus();
	}


	// Signals //

	private void OnPlayPressed()
	{
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


	// Other Functions //

	public void Focus()
	{
		_buttons.GetChild<Button>(0).GrabFocus();
	}
}
