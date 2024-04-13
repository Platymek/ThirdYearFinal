using Godot;
using System;

public partial class SaveFile : Resource
{
    // Properties //

    [Export] public bool UnlockedHardMode;
    [Export] public bool HowToPlayRead { get; private set; }


    // Forever Game Stats //

    [ExportGroup("Stats")]
    [Export] public int GamesWon { get; private set; }
    [Export] public int RoundsLost { get; private set; }
    [Export] public int RoundsWon { get; private set; }
    [Export] public int RoundsTotal { get; private set; }


    // Settings //

    [ExportGroup("Settings")]

    private bool _fullscreen;
    [Export] public bool Fullscreen 
    { 
        get => _fullscreen;
        set
        {
            _fullscreen = value;
            Save();
        }
    }


    // Stats for Current Saved Game //

    [Export] public int SavedRoundProgress;


    // Constructors //

    public SaveFile() : this(

        false,
        false,
        false,
        0,
        0,
        0,
        0,
        0) 
    { }

    public SaveFile(
        
        bool unlockedHardMode,
        bool howToPlayRead,
        bool fullscreen,
        int gamesWon,
        int roundsLost,
        int roundsWon,
        int totalRounds,
        int saveRoundProgress)
    {
        // bonus data
        UnlockedHardMode = unlockedHardMode;
        HowToPlayRead = howToPlayRead;
        Fullscreen = fullscreen;

        // stats
        GamesWon = gamesWon;
        RoundsLost = roundsLost;
        RoundsWon = roundsWon;
        RoundsTotal = totalRounds;

        // extra save data
        SavedRoundProgress = saveRoundProgress;
    }


    // modify properties and autosave //

    public void IncrementGamesWon()
    {
        GamesWon++;
        Save();
    }

    public void IncrementRoundsLost()
    {
        RoundsLost++;
        RoundsTotal++;
        Save();
    }

    public void IncrementRoundsWon()
    {
        RoundsWon++;
        RoundsTotal++;
        Save();
    }

    public void ConfirmHowToPlayRead()
    {
        HowToPlayRead = true;
        Save();
    }


    // Saving and Loading //

    private static string _savePath
        = "user://SaveFile.res";

    public static bool FileExists()
    {
        return FileAccess.FileExists(_savePath);
    }

    public static SaveFile Load()
    {
        if (FileExists())
        {
            return ResourceLoader.Load<SaveFile>(
                _savePath);
        }

        return null;
    }

    public void Save()
    {
        ResourceSaver.Save(
            this,
            _savePath);
    }
}
