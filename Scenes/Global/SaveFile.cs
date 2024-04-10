using Godot;
using System;

public partial class SaveFile : Resource
{
    // Properties //

    [Export] public bool UnlockedHardMode;


    // Forever Game Stats //

    public int GamesWon { get; private set; }
    public int RoundsLost { get; private set; }
    public int RoundsWon { get; private set; }


    // Settings //

    public bool Fullscreen;


    // Stats for Current Saved Game //

    public int SavedRoundProgress;


    // Constructors //

    public SaveFile() : this(

        false,
        false) 
    { }

    public SaveFile(
        
        bool unlockedHardMode,
        bool fullscreen)
    {
        // bonus data
        UnlockedHardMode = unlockedHardMode;
        Fullscreen = fullscreen;

        // stats
        GamesWon = 0;
        RoundsLost = 0;
        RoundsWon = 0;

        // extra save data
        SavedRoundProgress = 0;
    }


    // increment stats //

    public void IncrementGamesWon(int value = 1)
    {
        GamesWon += value;
        Save();
    }

    public void IncrementRoundsLost(int value = 1)
    {
        RoundsLost += value;
        Save();
    }

    public void IncrementRoundsWon(int value = 1)
    {
        RoundsWon += value;
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
