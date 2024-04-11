using Godot;
using System;

public partial class RoundNew : Menu
{
	[ExportGroup("Nodes")]
    [Export] private AttackOption _attack1;
    [Export] private AttackOption _attack2;
	[Export] private Label _title;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		base._Ready();


		// say "final round"
		if (Global.IsFinalRound())
        {
            _title.Text =
                $"- Final Round -";
        }
		// or say what number round it is
		else
        {
            _title.Text =
                $"- Round {Global.CurrentRoundProgress} -";
        }

        // set a random attack for the first attack option
        Attack randomAttack1 = Global.GetRandomAttack();
        
		_attack1.SetAttack(
			randomAttack1, 
			Global.GetHealthBonus(randomAttack1.AttackType), 
			Global.GetCurrentOpponentHealth());


        // set a random attack for the second attack option
        Attack randomAttack2 = Global.GetRandomAttack();

        _attack2.SetAttack(
			randomAttack2, 
			Global.GetHealthBonus(randomAttack2.AttackType), 
			Global.GetCurrentOpponentHealth());
    }
	
	// when an attack has been selected
	private void OnAttackPicked(Attack attack, float healthBonus)
	{
		// add it and automatically change scene to the game
        Global.AddOpponentAttack(attack, healthBonus);
		Global.ChangeScene(Global.Scene.Game);
	}
}
