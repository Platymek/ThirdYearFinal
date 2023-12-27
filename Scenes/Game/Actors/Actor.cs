using Godot;
using System;

public partial class Actor : CharacterBody3D
{
	// Properties //

	[Export] public Actor Target { protected set; get; }
	[Export] private ActorStats _stats;

	private string _state;
	[Export] public virtual string State
	{
		get
		{
			return _state;
		}

		protected set
		{
			if (_state == value) return;
			
			_state = value;

			_animationPlayer ??= GetNode<AnimationPlayer>("AnimationPlayer");
			_animationPlayer.Play(value);
				
			GD.Print($"{Name} change State to {value}");
		}
	}
	
	public Vector2 Position2D
	{
		get
		{
			Vector2 position = new Vector2(
				Position.X,
				Position.Z);

			return position;
		}

		set
		{
			Vector3 position = new Vector3(
				value.X,
				Position.Y,
				value.Y);

			Position = position;
		}
	}

	private AnimationPlayer _animationPlayer;
	private AttackStats _attackStats;

	public float Health;
	public float CurrentKnock;

	public float HealthPercentage
	{
		get => Health / _stats.MaxHealth;
	}

	// if true, disable JustHit, else make true
	private bool _justJustHit;
	public bool JustHit;

	float _previousDamage;
	float _previousKnock;
	
	
	// Node Functions //
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		// get nodes
		_attackStats = GetNode<AttackStats>("AttackStats");

		// initialise variables
		Health = _stats.MaxHealth;
		CurrentKnock = 0;

		_justJustHit = false;
		JustHit = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		// check hurtbox if set to
		if (_attackStats.CheckDamage && _attackStats.HurtBoxes != null)
		{
			CheckDamage();
		}

		// if just hit, set JustHit to true so that this can be seen next frame
		if (!_justJustHit)
		{
			_justJustHit = true;
			JustHit = true;
		}
		else
		{
			JustHit = false;
		}
	}
	
	
	// Other Functions //
	
	// check the current hit boxes to get if there have been any collisions and hurt actors
	void CheckDamage()
	{
		// check every overlapping area
		foreach (var nodePath in _attackStats.HurtBoxes)
		{
			foreach (var area3D in GetNode<Area3D>(nodePath).GetOverlappingAreas())
			{
				var victim = area3D.GetOwner<Node>();

				if (victim is not Actor actor) continue;
				
				if (actor.Name != Name)
				{
					actor.Hurt(
						_stats.DamageMultiplier * _attackStats.Damage,
						_stats.KnockMultiplier * _attackStats.Knock);
				}
			}
		}

		_attackStats.CheckDamage = false;
	}
	
	// get hurt by another actor
	void Hurt(float damage, float knockback)
	{
		float finalDamage = damage * _attackStats.DamageReactMutliplier;
		float finalKnockback = knockback * _attackStats.DamageReactMutliplier;

		Health -= finalDamage;
		CurrentKnock = finalKnockback;
		_justJustHit = false;

		_previousDamage = damage;
		_previousKnock = knockback;

		GD.Print($"{Name} was hit for {_attackStats.DamageReactMutliplier} damage " +
		         $"and {finalKnockback} knockback");
	}
	
	// rotate actor to face target actor
	void TrackTarget(float delta)
	{
		// get angle difference (the smallest angle to the target)

		float angleToTarget = new Vector2(-Position.Z, Position.X)
			                      .AngleToPoint(new Vector2(-Target.Position.Z, Target.Position.X))
		                      * -1;

		float angleDifference = Mathf.AngleDifference(Rotation.Y, angleToTarget);


		// if the difference is 0, no rotation is needed

		if (angleDifference != 0)
		{
			// rotate character

			float deltaAngle = Mathf.Tau * delta * _attackStats.TrackSpeedMultiplier * _stats.TrackSpeed;

			Vector3 newRotation = Rotation;

			newRotation.Y += angleDifference < 0 
				? -deltaAngle
				: deltaAngle;

			Rotation = newRotation;


			// get new angle difference

			float newAngleToTarget = new Vector2(-Position.Z, Position.X)
				                         .AngleToPoint(new Vector2(-Target.Position.Z, Target.Position.X))
			                         * -1;

			float newAngleDifference = Mathf.AngleDifference(Rotation.Y, newAngleToTarget);


			// if the sign has changed, it means that the character has faced the target
			// and point the character to the target accordingly

			if ((angleDifference < 0 && newAngleDifference > 0) ||
			    (angleDifference > 0 && newAngleDifference < 0))
			{
				Rotation = new Vector3(
					Rotation.X,
					newAngleToTarget,
					Rotation.Z);
			}
		}

	}
}
