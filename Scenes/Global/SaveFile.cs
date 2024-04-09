using Godot;
using System;

public partial class SaveFile : Resource
{
    // Properties //

    [Export] public bool UnlockedHardMode;


    // Forever Game Stats //

    public int NumberOfGames;
    public int NumberOfWins;
    public int RoundsWon;


    // Stats for Current Saved Game //

    public int SavedRoundProgress;


    // Constructors //

    public SaveFile() : this(

        false) 
    { }

    public SaveFile(
        
        bool unlockedHardMode)
    { 
        UnlockedHardMode = unlockedHardMode;

        NumberOfGames = 0;
        NumberOfWins = 0;
        RoundsWon = 0;

        SavedRoundProgress = 0;
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
