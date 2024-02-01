using Godot;
using System;

public partial class AttackOption : CenterContainer
{
	// Properties //

	private Attack _attack;
	private float _healthBonus;

	[Signal] public delegate void PickedEventHandler(Attack attack, float healthBonus);


	// Other Functions //

	public void SetAttack(Attack attack, float healthBonus, float currentOpponentHealth)
	{
		_attack = attack;
		_healthBonus = 1f + healthBonus;

		GD.Print(healthBonus);
		
		healthBonus = (int)(healthBonus * currentOpponentHealth * 100);

		GetNode<Label>("PanelContainer/VBoxContainer/HBoxContainer/Name").Text =
			$"{attack.Name}";

		if (healthBonus > 0)
		{
			GetNode<Label>("PanelContainer/VBoxContainer/HBoxContainer/HealthBonus").Text =
				$"+{healthBonus} Opponent Health";
		}
		else
		{
			GetNode<Label>("PanelContainer/VBoxContainer/HBoxContainer/HealthBonus").Text =
				$"";
		}

		GetNode<Label>("PanelContainer/VBoxContainer/Description").Text =
			attack.Description;
	}

	private void OnPickPressed()
	{
		EmitSignal(SignalName.Picked, _attack, _healthBonus);
	}
}
