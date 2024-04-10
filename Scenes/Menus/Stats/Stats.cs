using Godot;
using System;

public partial class Stats : Menu
{
	[ExportGroup("Nodes")]
	[Export] private Label _roundsWon;
	[Export] private Label _roundsLost;
	[Export] private Label _gamesWon;

	public override void _Ready()
	{
		base._Ready();


		// display all stats
		_roundsWon.Text = Global.SaveFile.RoundsWon.ToString();
		_roundsLost.Text = Global.SaveFile.RoundsLost.ToString();
		_gamesWon.Text = Global.SaveFile.GamesWon.ToString();
	}
}
