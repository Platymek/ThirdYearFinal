using Godot;
using System;

public partial class PlayerStats : ActorStats
{
    [Export] public float DodgeSpeedMultiplier = 3;
    [Export] public float DodgeDuration = .2f;
    [Export] public float DodgeStunDuration = .2f;


    // Saving and Loading //

    private static string _savePath
        = "user://PlayerStats.res";

    public static bool FileExists()
    {
        return FileAccess.FileExists(_savePath);
    }

    public static PlayerStats Load()
    {
        if (FileExists())
        {
            var save = ResourceLoader.Load<PlayerStats>(
                _savePath);

            // delete the file after loading
            DirAccess.RemoveAbsolute(_savePath);

            return save;
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
