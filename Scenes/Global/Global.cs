using Godot;
using System;
using Godot.Collections;
using static Global;

public partial class Global : Node
{
    // Properties //

    [Export] public SaveFile SaveFile;

	[ExportGroup("Game Constants")]
	[Export] public int RoundsToWin = 6;
    [Export] private Array<Opponent.AttackTypes> _forcedTypeSelection;
    [Export] private Array<bool> _forcedTypeSelectionEnabled;

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


    // Settings //

    private bool _fullscreen;
    public bool Fullscreen
    {
        get => _fullscreen;

        set
        {
            _fullscreen = value;
            SaveFile.Fullscreen = value;


            if (Fullscreen)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                return;
            }

            // else, set to windowed
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }


	// array of the currently eligible attack type categories
	// to pull random attacks from
	private Array<Opponent.AttackTypes> _eligibleAttackTypes;


    // Node Functions //

    public override void _Ready()
	{
		base._Ready();

		_opponentUniqueAttacks = GetNode("OpponentUniqueAttacks");


        // if a save file has been specified, then the user
        // is debugging and therefore a savefile need not be loaded
        if (SaveFile != null) return;

        // load main save file
        if (SaveFile.FileExists())
        {
            SaveFile = SaveFile.Load();


            // Load Settings //

            Fullscreen = SaveFile.Fullscreen;
        }
        else
        {
            SaveFile = new SaveFile();
            SaveFile.Save();
        }
    }

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("fullscreen"))
        {
            // toggle fullscreen
            Fullscreen = !Fullscreen;
        }
	}


	// Functions //

    // Preparing the Game //

	// prepare starting stats to get ready to start the game
	public void PrepareNewGame()
    {
        InitialiseEligibleAttackTypes();

        CurrentRoundProgress = 1;

        if (!Debug)
        {
            // duplicate the list of reimaing attacks as to not affect future games
            RemainingOpponentUniqueAttacks = _opponentUniqueAttacks.Duplicate();

            // add new attack to each of the specified categories
            foreach (
                Opponent.AttackTypes attackType
                in new Opponent.AttackTypes[] 
                {
			        Opponent.AttackTypes.FarFromWall, 
                    Opponent.AttackTypes.MixUp
                })
            {
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


	// Data Management //

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


	// Attacks //

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

    // select a random attack from a specific category and remove it from its list
    public Attack GetRandomAttack(Opponent.AttackTypes attackType)
    {
        GD.Randomize();

        // get a random attack from the specified category
        Node categoryNode = RemainingOpponentUniqueAttacks.GetNode(attackType.ToString());
        Attack newAttack = categoryNode.GetChildren().PickRandom() as Attack;
        Attack newAttackDuplicate = newAttack.Duplicate() as Attack;

        // remove attack from remaining list and return name
        newAttack.Free();

        // check the category is still eligible
        CheckEligibleAndDelete(attackType);

        return newAttackDuplicate;
    }

    // select a random attack and remove it from its list
    public Attack GetRandomAttack()
    {
        GD.Randomize();

        // the round number has to have 2 taken away as it is the second round
        // and round number starts with 1
        int roundOffset = 2;

        // pick a random category
        Opponent.AttackTypes randomAttackType
            = _forcedTypeSelectionEnabled[CurrentRoundProgress - roundOffset]
            ? _forcedTypeSelection[CurrentRoundProgress - roundOffset]
            : _eligibleAttackTypes.PickRandom();

        return GetRandomAttack(randomAttackType);
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


    // Rounds //

    // end round on a win
    public void EndRoundWin()
    {
        // if the player just won the final round
        if (IsFinalRound())
        {
            ChangeScene(Scene.YouWon);

            return;
        }

        // if not the final round, progress and save
        CurrentRoundProgress++;
        SaveFile.SavedRoundProgress++;
        SaveFile.IncrementRoundsWon();

        Save();
        ChangeScene(Scene.RoundEnd);
    }

    // end round on a loss
    public void EndRoundLoss()
    {
        SaveFile.IncrementRoundsLost();
        ChangeScene(Scene.YouLost);
    }

    public bool IsFinalRound()
	{
		return CurrentRoundProgress == RoundsToWin;
	}


    // Changing Scene //

	public enum Scene
	{
		Game,
		MainMenu,
        RoundEnd,
		RoundNew,
        YouLost,
        YouWon,
        Stats,
        HowToPlay,
    }

	public void ChangeScene(Scene scene)
	{
		switch (scene)
		{
			case Scene.Game:

				GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");

				break;


			case Scene.MainMenu:

				GetTree().ChangeSceneToFile("res://Scenes/Menus/MainMenu/MainMenu.tscn");

				break;


			case Scene.RoundEnd:

				GetTree().ChangeSceneToFile("res://Scenes/Menus/RoundEnd/RoundEnd.tscn");

				break;


			case Scene.RoundNew:

				GetTree().ChangeSceneToFile("res://Scenes/Menus/RoundNew/RoundNew.tscn");

				break;


            case Scene.YouLost:

                GetTree().ChangeSceneToFile("res://Scenes/Menus/YouLost/YouLost.tscn");

                break;


            case Scene.YouWon:

                GetTree().ChangeSceneToFile("res://Scenes/Menus/YouWon/YouWon.tscn");

                break;


            case Scene.Stats:

                GetTree().ChangeSceneToFile("res://Scenes/Menus/Stats/Stats.tscn");

                break;


            case Scene.HowToPlay:

                GetTree().ChangeSceneToFile("res://Scenes/Menus/HowToPlay/HowToPlay.tscn");

                break;
        }
	}
}
