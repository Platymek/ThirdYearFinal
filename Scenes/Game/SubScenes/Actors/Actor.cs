using Godot;
using System;
using System.Collections.Generic;

public partial class Actor : CharacterBody3D
{
	// Properties //

	[Signal] public delegate void DeathEventHandler();

	[Export] public Node3D Target { protected set; get; }
	[Export] protected ActorStats _stats;
	
	private AnimationPlayer _animationPlayer;
	protected AttackStats AttackStats;

	private Label3D _stateLabel;
	private bool _bounced = false;

	protected virtual string Animation
	{
		get => _animationPlayer.CurrentAnimation;
		set
		{
			_animationPlayer?.Play(value);
		}
	}

	protected double GetAnimationCurrentLength()
	{
		return _animationPlayer.CurrentAnimationLength;
	}
	
	// the state the actor is in
	private string _state;
	[Export] public virtual string State
	{
		get
		{
			return _state;
		}

		protected set
		{
			if (_state == "death") return;


			_state = value;

			if (_animationPlayer?.HasAnimation(value) == true)
			{
				Animation = value;
			}
			
			if (_stateLabel != null)
			{
				_stateLabel.Text = value;
			}

			
			if (AttackStats != null)
			{
				switch (value)
				{
					case "idle":
						AttackStats.TrackSpeedMultiplier = 1;
						_bounced = false;
						break;
					
					case "stun_start":
						AttackStats.TrackSpeedMultiplier = 0;
						break;
				}
			}
			
			GD.Print($"{Name} change State to {value}");
		}
	}
	
	// position of actor in a top-down, 2D space
	public Vector2 Position2D
	{
		get
		{
			return Vector3To2D(Position);
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
		AttackStats = GetNode<AttackStats>("AttackStats");
		_stateLabel = GetNode<Label3D>("StateLabel");

		// initialise variables

		Health = 1;

		CurrentKnock = 0;
		_justJustHit = false;
		JustHit = false;
		
		// get animation player and start animation based on starting state
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		// move using velocity from previous frame
		MoveAndSlide();
		
		// reset velocity
		Velocity = Vector3.Zero;

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


		float fDelta = (float)delta;

		if (CurrentKnock != 0)
		{
			ProcessKnock(fDelta);
		}
		

		TrackTarget(fDelta);
	}
	
	// get hurt by another actor
	protected virtual void Hurt(float damage, float knock)
	{
		float finalDamage = damage * _stats.DamageReactMultiplier * AttackStats.DamageReactMutliplier;
		float finalKnock = knock * _stats.KnockReactMultiplier * AttackStats.KnockReactMutliplier;

		Health -= finalDamage;
		CurrentKnock = finalKnock;
		_justJustHit = false;

		_previousDamage = damage;
		_previousKnock = knock;

		GD.Print($"{Name} was hit for {AttackStats.DamageReactMutliplier} damage " +
				 $"and {finalKnock} knock, with only {HealthPercentage}% health left");

		if (Health <= 0)
		{
			State = "death";
		}

		if (CurrentKnock == 0) return;
		State = "stun_start";
	}
	
	// get hurt by another actor and change angle
	protected void Hurt(float damage, float knock, float angle)
	{
		Rotation = new Vector3(Rotation.X, angle, Rotation.Z);
		
		Hurt(damage, knock);
	}

	// returns true if the wall bounce was successful
	public bool WallBounce(float angle)
	{
		// if already bounced or no knockback, do not wall bounce
		if (CurrentKnock == 0) return false;
		if (_bounced) return false;
		
		float angleDifference = angle - Rotation.Y;
		float hurtAngle = angle + angleDifference;
		
		Hurt(_previousDamage, -CurrentKnock / (_stats.KnockReactMultiplier * AttackStats.KnockReactMutliplier), hurtAngle);
		State = "stun";
			
		GD.Print($"Actor {Name} Wall Bounced!");
		
		// can only bounce once
		_bounced = true;
		return true;
	}

	static protected float GetAngleDifference(Vector3 myPosition, float myRotation, 
		Vector3 targetPosition, float tolerance = 0)
	{
		// get angle difference (the smallest angle to the target)

		float angleToTarget = new Vector2(-myPosition.Z, myPosition.X)
								  .AngleToPoint(new Vector2(-targetPosition.Z, targetPosition.X))
							  * -1;

		float angleDifference = Mathf.AngleDifference(myRotation, angleToTarget);

		if (angleDifference < tolerance && angleDifference > -tolerance)
		{
			return 0;
		}

		return angleDifference;
	}

	protected float GetAngleDifference(Vector3 targetPosition, float tolerance = 0)
	{
		return GetAngleDifference(Position, Rotation.Y, targetPosition, tolerance);
	}
	
	// rotate actor to face target actor
	private void TrackTarget(float delta)
	{
		float angleDifference = GetAngleDifference(Target.Position);

		// if the difference is 0, no rotation is needed

		if (angleDifference != 0)
		{
			// rotate character

			float deltaAngle = Mathf.Tau * delta * AttackStats.TrackSpeedMultiplier * _stats.TrackSpeed;

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
	
	// move on local rotation
	protected void Move(Vector2 velocity)
	{
		// get velocity
		Velocity += (new Vector3(velocity.X, 0, -velocity.Y) * _stats.Speed).Rotated(Vector3.Up, Rotation.Y);
	}

	protected static Vector2 Vector3To2D(Vector3 vector)
	{
		return new Vector2(
			vector.X,
			vector.Z);
	}
	
	void ProcessKnock(float delta)
	{
		// move player back for the value of the current knock
		Move(new Vector2(0, -CurrentKnock));

		bool positiveKnock = CurrentKnock >= 0;

		// move knock value to zero
		CurrentKnock -= delta * _stats.KnockReactDeceleration * _stats.KnockReactMultiplier
								  * (positiveKnock ? 1 : -1);

		bool newPositiveKnock = CurrentKnock >= 0;

		// if the sign has changed on the knock, set to 0 as it has crossed 0
		if (positiveKnock != newPositiveKnock)
		{
			CurrentKnock = 0;

			// if the actor's state is 'stun', turn the player back to idle
			if (State == "stun" || State == "stun_start")
			{
				State = "idle";
			}
		}
	}

	private void OnActorHurt(Actor actor)
	{
		// if actor is self or not checking for damage currently, return
		if (actor.Name == Name || !AttackStats.CheckDamage) return;

		// stop checking damage
		AttackStats.CheckDamage = false;
		
		// hurt actor
		actor.Hurt(
			_stats.DamageMultiplier * AttackStats.Damage,
			_stats.KnockMultiplier * AttackStats.Knock);
	}

	private void EmitDeathSignal()
	{
		EmitSignal(SignalName.Death);
	}
}
