using Godot;
using System;
using Godot.Collections;

public partial class Global : Node
{
	// Properties //

	[Export] public SaveFile SaveFile;

	[ExportGroup("Game Constants")]
	[Export] public int RoundsToWin = 6;
	[Export] private Array<Opponent.AttackTypes> _forcedTypeSelection;
	[Export] private Array<bool> _forcedTypeSelectionEnabled;
	[Export] private float _healthBonusMultiplier = 0.2f;

	[ExportGroup("Default Stats")]
	[Export] private PlayerStats _playerStatsDefault;
	[Export] private OpponentStats _opponentStatsDefault;
	public PlayerStats PlayerStats;
	public OpponentStats OpponentStats;

	[ExportGroup("Debug")]
	[Export] public bool Debug = false;
	[Export] public PlayerStats DebugPlayerStats;
	[Export] public OpponentStats DebugOpponentStats;


	// For Current Game //

	private Node _opponentUniqueAttacks;
	public Attack LastAddedAttack;
	public int CurrentRoundProgress;

	public Node RemainingUniqueAttacks;
	private Node _temporaryUniqueAttacks;
	private OpponentStats _savedOpponentStats;


    // array of the currently eligible attack type categories
    // to pull random attacks from
    private Array<Opponent.AttackTypes> _eligibleAttackTypes;
	private int _randomTypeIndex;

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
				DisplayServer.WindowSetMode(
					DisplayServer.WindowMode.ExclusiveFullscreen);
				return;
			}

			// else, set to windowed
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
	}


	// Node Functions //

	public override void _Ready()
	{
		base._Ready();

		_opponentUniqueAttacks = GetNode("OpponentUniqueAttacks");

		RefreshActorStats();

		// if a save file has been specified, then the user
		// is debugging and therefore a savefile need not be loaded
		if (SaveFile != null) return;

		// load main save file
		if (SaveFile.FileExists())
		{
			SaveFile = SaveFile.Load();


			// Load Settings //

			Fullscreen = SaveFile.Fullscreen;

			GD.Print(SaveFile.RoundsWon);
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
			PrepareRemainingUniqueAttacks();
			PrepareTemporaryUniqueAttacks();
			RefreshActorStats();


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
		PrepareRemainingUniqueAttacks();

		// function can only be called when these files exist anyway
		LoadActorStats();
		CurrentRoundProgress = SaveFile.SavedRoundProgress;


		// remove the already equipped unique attacks
		foreach (Opponent.AttackTypes attackType 
			in OpponentStats.CurrentUniqueAttacks.Keys) 
		{
			foreach (string attack 
				in OpponentStats.CurrentUniqueAttacks[attackType])
			{
				// find node in category and remove
				RemainingUniqueAttacks
					.GetNode(attackType.ToString())
					.GetNode(attack)
					.Free();
			}

			// check the category is still eligible
			CheckEligibleAndDelete(attackType);
		}

		PrepareTemporaryUniqueAttacks();
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

		DeleteAttack(attack);
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
		if (RemainingUniqueAttacks
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
		Node categoryNode = _temporaryUniqueAttacks.GetNode(
			attackType.ToString());
			
		Attack newAttack = categoryNode.GetChildren().PickRandom() as Attack;
		Attack newAttackDuplicate = newAttack.Duplicate() as Attack;

		// remove attack from remaining list and return name
		newAttack.Free();

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
			: _eligibleAttackTypes.Count > 1
			? _eligibleAttackTypes[_randomTypeIndex++]
			: _eligibleAttackTypes[0];

		return GetRandomAttack(randomAttackType);
	}

	// delete an attack from the remaining types
	public void DeleteAttack(Attack attack)
	{
		RemainingUniqueAttacks
			.GetNode(attack.AttackType.ToString())
			.GetNode(attack.Name.ToString())
			.Free();

		// check the category is still eligible
		CheckEligibleAndDelete(attack.AttackType);
	}

	public float GetHealthBonus(Opponent.AttackTypes category)
	{
		int numberOfUniqueAttacks 
			= OpponentStats.CurrentUniqueAttacks[category].Count + 1;
			
		return _healthBonusMultiplier * numberOfUniqueAttacks;
	}

	public float GetCurrentOpponentHealth()
	{
		return OpponentStats.MaxHealth;
	}

	private void RefreshActorStats()
	{
		PlayerStats = _playerStatsDefault.Duplicate() as PlayerStats;
		OpponentStats = _opponentStatsDefault.Duplicate() as OpponentStats;

		InitialiseEligibleAttackTypes();
	}

	private void LoadActorStats()
	{
		PlayerStats = PlayerStats.Load();
		OpponentStats = OpponentStats.Load();

		GD.Print(OpponentStats.CurrentUniqueAttacks);
	}

	private void PrepareRemainingUniqueAttacks()
	{
		// if remaining unique attacks node already exists, free it
		if (RemainingUniqueAttacks != null)
		{
			RemainingUniqueAttacks.Free();
		}

		RemainingUniqueAttacks
			= _opponentUniqueAttacks
			.Duplicate();
	}

	private void PrepareTemporaryUniqueAttacks()
	{
		_randomTypeIndex = 0;
		_eligibleAttackTypes.Shuffle();

        if (_temporaryUniqueAttacks != null)
		{
			_temporaryUniqueAttacks.Free();
		}

		_temporaryUniqueAttacks 
			= RemainingUniqueAttacks
			.Duplicate();
	}


	// Rounds //

	// end round on a win
	public void EndRoundWin()
	{
		// if the player just won the final round
		if (IsFinalRound())
		{
			SaveFile.IncrementGamesWon();
			ChangeScene(Scene.YouWon);

			return;
		}

		// if not the final round, progress and save
		CurrentRoundProgress++;
		SaveFile.SavedRoundProgress++;
		SaveFile.IncrementRoundsWon();

		PrepareTemporaryUniqueAttacks();

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

				GetTree().ChangeSceneToFile(
					"res://Scenes/Game/Game.tscn");

				break;


			case Scene.MainMenu:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/MainMenu/MainMenu.tscn");

				break;


			case Scene.RoundEnd:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/RoundEnd/RoundEnd.tscn");

				break;


			case Scene.RoundNew:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/RoundNew/RoundNew.tscn");

				break;


			case Scene.YouLost:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/YouLost/YouLost.tscn");

				break;


			case Scene.YouWon:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/YouWon/YouWon.tscn");

				break;


			case Scene.Stats:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/Stats/Stats.tscn");

				break;


			case Scene.HowToPlay:

				GetTree().ChangeSceneToFile(
					"res://Scenes/Menus/HowToPlay/HowToPlay.tscn");

				break;
		}
	}
}
