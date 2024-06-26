using Godot;
using System;

public partial class ActorStats : Resource
{
    // Properties //

    [ExportGroup("Stats")]
    [Export] public float MaxHealth;
    [Export] public float Speed;
    [Export] public float TrackSpeed;


    // Damage //

    [ExportSubgroup("Damage")]
    [Export] public float DamageMultiplier;
    [Export] public float DamageReactMultiplier;


    // Knock //

    [ExportSubgroup("Knock")]
    [Export] public float KnockMultiplier;
    [Export] public float KnockReactMultiplier;
    [Export] public float KnockReactDeceleration;
    

    // Constructors //

    public ActorStats(float maxHealth, float speed, float damageMultiplier, float damageReactMultiplier, 
        float knockMultiplier, float knockReactMultiplier, float knockReactDeceleration)
    {
        MaxHealth = maxHealth;
        Speed = speed;
        
        DamageMultiplier = damageMultiplier;
        DamageReactMultiplier = damageReactMultiplier;

        KnockMultiplier = knockMultiplier;
        KnockReactMultiplier = knockReactMultiplier;
        KnockReactDeceleration = knockReactDeceleration;
    }

    public ActorStats() : this(1, 1, 1, 1, 1, 1, 1)
    {
        // initialise values to 1
    }
}
