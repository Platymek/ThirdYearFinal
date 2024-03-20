using Godot;
using System;

public partial class PlayerAttackStats : AttackStats
{
    public enum PunchChargeStates
    {
        None,
        Light,
        Medium,
        Heavy,
    }

    [Export] public PunchChargeStates PunchChargeState;
    [Export] public bool Cancellable = true;
}
