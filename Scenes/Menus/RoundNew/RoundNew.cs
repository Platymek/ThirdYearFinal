using Godot;
using System;

public partial class RoundNew : Menu
{
	// Properties //

	private Global _global;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		_global = GetNode<Global>("/root/Global");


		Attack randomAttack1 = _global.GetRandomAttack();

		GetNode<AttackOption>("AspectRatioContainer/CenterContainer/PanelContainer/VBoxContainer/CenterContainer/Buttons/Attack1")
			.SetAttack(randomAttack1, _global.GetHealthBonus(randomAttack1.AttackType), _global.GetCurrentOpponentHealth());


		Attack randomAttack2 = _global.GetRandomAttack();

		GetNode<AttackOption>("AspectRatioContainer/CenterContainer/PanelContainer/VBoxContainer/CenterContainer/Buttons/Attack2")
			.SetAttack(randomAttack2, _global.GetHealthBonus(randomAttack2.AttackType), _global.GetCurrentOpponentHealth());
	}
	
	private void OnAttackPicked(Attack attack, float healthBonus)
	{
		_global = GetNode<Global>("/root/Global");

		_global.AddOpponentAttack(attack, healthBonus);

		GetTree().ChangeSceneToFile("res://Scenes/Game/Game.tscn");
	}
}
