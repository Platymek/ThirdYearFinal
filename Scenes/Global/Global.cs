using Godot;
using System;
using System.Security.Cryptography;

public partial class Global : Node
{
	// Properties //

	[Export] public PlayerStats PlayerStats;
	[Export] public OpponentStats OpponentStats;

	private Node _opponentUniqueAttacks;
	public Node RemainingOpponentUniqueAttacks;
	

	// Node Functions //

	public override void _Ready()
	{
		base._Ready();

		_opponentUniqueAttacks = GetNode("OpponentUniqueAttacks");


		PrepareGame();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("fullscreen"))
		{
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen)
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			}
			else
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
			}
		}
	}


	// Functions //

	// prepare starting stats to get ready to start the game
	public void PrepareGame()
	{
		Node remainingOpponentUniqueAttacks = _opponentUniqueAttacks.Duplicate();
		remainingOpponentUniqueAttacks.Name = "RemainingOpponentUniqueAttacks";

		AddChild(remainingOpponentUniqueAttacks, true);
		GD.Print(GetChildren());
		RemainingOpponentUniqueAttacks = GetNode("RemainingOpponentUniqueAttacks");

		foreach (Opponent.AttackTypes attackType in new Opponent.AttackTypes[] { 
			Opponent.AttackTypes.CloseToWall, Opponent.AttackTypes.Neutral, Opponent.AttackTypes.FarFromWall })
		{
			// add new attack to each of the specified categories

			string newAttack = GetRandomAttack(attackType);
			OpponentStats.CurrentUniqueAttacks[attackType].Add(newAttack);
		}

		GD.Print(OpponentStats.CurrentUniqueAttacks);
	}

	public string GetRandomAttack(Opponent.AttackTypes attackType)
	{
		GD.Randomize();

		// get a random attack from the specified category
		Node categoryNode = RemainingOpponentUniqueAttacks.GetNode(attackType.ToString());
		Node newAttack = categoryNode.GetChild((int)(GD.Randi() % categoryNode.GetChildCount()));
		string newAttackName = newAttack.Name;

		// remove attack from remaining list and return name
		newAttack.QueueFree();
		return newAttackName;
	}
}
