using Godot;
using System;

public partial class SaveFile : Resource
{
    // Properties //

    [Export] public bool UnlockedHardMode;
    [Export] public bool HowToPlayRead { get; private set; }


    // Forever Game Stats //

    public int GamesWon { get; private set; }
    public int RoundsLost { get; private set; }
    public int RoundsWon { get; private set; }
    public int TotalRounds { get; private set; }


    // Settings //

    [Export] public bool Fullscreen { get; private set; }


    // Stats for Current Saved Game //

    public int SavedRoundProgress;


    // Constructors //

    public SaveFile() : this(

        false,
        false,
        false) 
    { }

    public SaveFile(
        
        bool unlockedHardMode,
        bool howToPlayRead,
        bool fullscreen)
    {
        // bonus data
        UnlockedHardMode = unlockedHardMode;
        HowToPlayRead = howToPlayRead;
        Fullscreen = fullscreen;

        // stats
        GamesWon = 0;
        RoundsLost = 0;
        RoundsWon = 0;
        TotalRounds = 0;

        // extra save data
        SavedRoundProgress = 0;
    }


    // modify properties and autosave //

    public void IncrementGamesWon(int value = 1)
    {
        GamesWon += value;
        Save();
    }

    public void IncrementRoundsLost(int value = 1)
    {
        RoundsLost += value;
        TotalRounds += value;
        Save();
    }

    public void IncrementRoundsWon(int value = 1)
    {
        RoundsWon += value;
        TotalRounds += value;
        Save();
    }

    public void ConfirmHowToPlayRead()
    {
        HowToPlayRead = true;
        Save();
    }

    public void ToggleFullscreen()
    {
        Fullscreen = !Fullscreen;
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
