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

	public Attack LastAddedAttack;
	

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
			//Opponent.AttackTypes.CloseToWall, Opponent.AttackTypes.Neutral, Opponent.AttackTypes.FarFromWall })
			Opponent.AttackTypes.MixUp })
		{
			// add new attack to each of the specified categories

			AddOpponentAttack(GetRandomAttack(attackType));
		}

		GD.Print(OpponentStats.CurrentUniqueAttacks);
	}

	// select a random attack from a specific category and remove it from its list
	public Attack GetRandomAttack(Opponent.AttackTypes attackType)
	{
		GD.Randomize();

		// get a random attack from the specified category
		Node categoryNode = RemainingOpponentUniqueAttacks.GetNode(attackType.ToString());
		Attack newAttack = categoryNode.GetChild<Attack>((int)(GD.Randi() % categoryNode.GetChildCount()));
		Attack newAttackDuplicate = newAttack.Duplicate() as Attack;

		// remove attack from remaining list and return name
		newAttack.QueueFree();

		// delete category node if empty
		if (categoryNode.GetChildCount() == 0)
		{
			categoryNode.Free();
		}

		return newAttackDuplicate;
	}

	// select a random attack and remove it from its list
	public Attack GetRandomAttack()
	{
		// pick a random category
		GD.Randomize();
		int nodeCategoryIndex = (int)(GD.Randi() % RemainingOpponentUniqueAttacks.GetChildCount());
		Node categoryNode = RemainingOpponentUniqueAttacks.GetChild(nodeCategoryIndex);

		// pick a random attack from the category
		GD.Randomize();
		Attack newAttack = categoryNode.GetChild<Attack>((int)(GD.Randi() % categoryNode.GetChildCount()));
		Attack newAttackDuplicate = newAttack.Duplicate() as Attack;

		// remove attack from remaining list and return name
		newAttack.Free();

		// delete category node if empty
		if (categoryNode.GetChildCount() == 0)
		{
			categoryNode.Free();
		}

		return newAttackDuplicate;
	}
	
	// end round on a win
	public void EndRoundWin()
	{
		Save();
		GetTree().ChangeSceneToFile("res://Scenes/Menus/RoundEnd/RoundEnd.tscn");
	}

	// end round on a loss
	public void EndRoundLoss()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu/MainMenu.tscn");
	}

	// save game
	public void Save()
	{
		// save opponent stats and player stats
		ResourceSaver.Save(PlayerStats, "user://PlayerStats.res");
		ResourceSaver.Save(OpponentStats, "user://OpponentStats.res");
	}

	public void Load()
	{
		// load files
		PlayerStats = ResourceLoader.Load<PlayerStats>("user://PlayerStats.res");
		OpponentStats = ResourceLoader.Load<OpponentStats>("user://OpponentStats.res");

		// delete files
		DirAccess.RemoveAbsolute("user://PlayerStats.res");
		DirAccess.RemoveAbsolute("user://OpponentStats.res");
	}

	public float GetHealthBonus(Opponent.AttackTypes category)
	{
		int numberOfUniqueAttacks = OpponentStats.CurrentUniqueAttacks[category].Count + 1;
		return 0.25f * numberOfUniqueAttacks;
	}

	public float GetCurrentOpponentHealth()
	{
		return OpponentStats.MaxHealth;
	}

	public void AddOpponentAttack(Attack attack)
	{
		LastAddedAttack = attack;
		OpponentStats.CurrentUniqueAttacks[attack.AttackType].Add(attack.Name);
	}

	public void AddOpponentAttack(Attack attack, float healthBonus)
	{
		AddOpponentAttack(attack);

		OpponentStats.MaxHealth *= healthBonus;

		GD.Print(OpponentStats.CurrentUniqueAttacks);
		GD.Print(OpponentStats.MaxHealth);
	}
}
