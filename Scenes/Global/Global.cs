using Godot;
using System;
using Godot.Collections;

public partial class Global : Node
{
    // Properties //

    [Export] public SaveFile SaveFile;

	[ExportGroup("Game Constants")]
	[Export] public int RoundsToWin = 6;

	[ExportGroup("Default Stats")]
    [Export] public PlayerStats PlayerStats;
	[Export] public OpponentStats OpponentStats;

	[ExportGroup("Debug")]
	[Export] public bool Debug = false;
    [Export] public PlayerStats DebugPlayerStats;
    [Export] public OpponentStats DebugOpponentStats;


	// For Current Game //

    private Node _opponentUniqueAttacks;
	public Node RemainingOpponentUniqueAttacks;
	public Attack LastAddedAttack;
	public int CurrentRoundProgress;

	// array of the currently eligible attack type categories
	// to pull random attacks from
	private Array<Opponent.AttackTypes> _eligibleAttackTypes;


    // Node Functions //

    public override void _Ready()
	{
		base._Ready();

		_opponentUniqueAttacks = GetNode("OpponentUniqueAttacks");


        // load main save file
        if (SaveFile.FileExists())
        {
            SaveFile = SaveFile.Load();
        }
        else
        {
            SaveFile = new SaveFile();
        }
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
	public void PrepareNewGame()
    {
        InitialiseEligibleAttackTypes();

        CurrentRoundProgress = 1;

        if (!Debug)
        {
            RemainingOpponentUniqueAttacks = _opponentUniqueAttacks.Duplicate();

            foreach (Opponent.AttackTypes attackType in new Opponent.AttackTypes[] { 
			//Opponent.AttackTypes.CloseToWall, Opponent.AttackTypes.Neutral, Opponent.AttackTypes.FarFromWall })
			Opponent.AttackTypes.FarFromWall, Opponent.AttackTypes.MixUp })
            {
                // add new attack to each of the specified categories

                AddOpponentAttack(GetRandomAttack(attackType));
            }

            GD.Print(OpponentStats.CurrentUniqueAttacks);
			return;
        }

		// if debug mode, set the opponent and player stats to their debug stats
        OpponentStats = DebugOpponentStats;
        PlayerStats = DebugPlayerStats;
    }

	// prepare for the player to continue the game
	public void PrepareContinueGame()
    {
        InitialiseEligibleAttackTypes();

        // function can only be called when these files exist anyway
        PlayerStats = PlayerStats.Load();
		OpponentStats = OpponentStats.Load();
		CurrentRoundProgress = SaveFile.SavedRoundProgress;

        RemainingOpponentUniqueAttacks = _opponentUniqueAttacks.Duplicate();

		// remove the already equipped unique attacks
        foreach (Opponent.AttackTypes attackType 
			in OpponentStats.CurrentUniqueAttacks.Keys) 
		{
			foreach (string attack 
				in OpponentStats.CurrentUniqueAttacks[attackType])
			{
				// find node in category and remove
				RemainingOpponentUniqueAttacks
					.GetNode(attackType.ToString())
					.GetNode(attack)
					.QueueFree();
			}

			// check the category is still eligible
			CheckEligibleAndDelete(attackType);
		}
    }

	// select a random attack from a specific category and remove it from its list
	public Attack GetRandomAttack(Opponent.AttackTypes attackType)
	{
		GD.Randomize();

		// get a random attack from the specified category
		Node categoryNode = RemainingOpponentUniqueAttacks.GetNode(attackType.ToString());
		Attack newAttack = categoryNode.GetChildren().PickRandom() as Attack;
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
        GD.Randomize();

		// pick a random category
		int nodeCategoryIndex = (int)_eligibleAttackTypes.PickRandom();
		Node categoryNode = RemainingOpponentUniqueAttacks.GetChild(nodeCategoryIndex);

		// pick a random attack from the category
		Attack newAttack = categoryNode.GetChildren()
			.PickRandom() 
			as Attack;

		Attack newAttackDuplicate = newAttack.Duplicate() 
			as Attack;

		// remove attack from remaining list and return name
		newAttack.Free();

		// check the category is still eligible
		CheckEligibleAndDelete(
			(Opponent.AttackTypes)nodeCategoryIndex);

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
        PlayerStats.Save();
		OpponentStats.Save();

		// save SaveFile with current round progress
		SaveFile.SavedRoundProgress = CurrentRoundProgress;
		SaveFile.Save();
    }

	// load player and opponent stats
	public void Load()
	{
		// load files
		PlayerStats = PlayerStats.Load();
		OpponentStats = OpponentStats.Load();
    }

	public bool HasSave()
	{
		return PlayerStats.FileExists()
			&& OpponentStats.FileExists();
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

	private void InitialiseEligibleAttackTypes()
	{
		_eligibleAttackTypes = new()
        {
            Opponent.AttackTypes.CloseToWall,
            Opponent.AttackTypes.Neutral,
            Opponent.AttackTypes.FarFromWall,
            Opponent.AttackTypes.MixUp,
        };
    }

	// check if an attack category has any children, otherwise
	// mark as ineligible by deleting from eligible list
	private void CheckEligibleAndDelete(Opponent.AttackTypes attackType)
	{
        // if no more children in one category,
        // delete category from eligible list
        if (RemainingOpponentUniqueAttacks
                .GetNode(attackType.ToString())
                .GetChildren()
                .Count == 0)
        {
            _eligibleAttackTypes.Remove(attackType);
        }
    }
}
