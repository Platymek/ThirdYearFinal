using Godot;
using System;
using System.Collections.Generic;

public partial class AttackStats : Node
{
	[Export] public float Damage;
	[Export] public float Knock;
	[Export] public bool CheckDamage = false;
	[Export] public float TrackSpeedMultiplier = 1;
	[Export] public float DamageReactMutliplier = 1;

	private NodePath[] _hurtBoxes;

	[Export]
	public NodePath[] HurtBoxes
	{
		get
		{
			return _hurtBoxes;
		}
		set
		{
			_hurtBoxes = Array.Empty<NodePath>();
			List<NodePath> hurtBoxes = new();
			
			foreach (NodePath n in value)
			{
				string hitBoxString = n.ToString();
				
				if (n == null || hitBoxString.Length < 3)
				{
					hurtBoxes.Add(new NodePath(hitBoxString.Substring(3)));
				}
			}
		}
	}
}
