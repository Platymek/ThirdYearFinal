using Godot;
using Godot.Collections;
using System;

public partial class Opponent : Actor
{
	// Properties //

	[Export] private string _startState;

	private OpponentStats _opponentStats;
	private OpponentAttackStats _opponentAttackStats;
	private Label3D _uniqueAttackLabel;
	private ActorModel _model;
	private AttackTypes _currentAttackType;
	private bool _strafeRight;

	private int _sumoCount;
	private int _sumoLimit;
	private int _last3Hit = 0;

	public override string State
	{
		get => base.State;
		
		protected set
		{
			if (_opponentAttackStats == null) return;
			if (State == "death") return;

			_opponentAttackStats.ClosingInSpeed = 0;
			_opponentAttackStats.StrafingSpeed = 0;
			_opponentAttackStats.DamageReactMutliplier = 1;
			_opponentAttackStats.KnockReactMutliplier = 1;

			_model.StandardMaterial3D.AlbedoColor = new Color("FFA300");

			string state = value;

			switch (value)
			{
				case "idle":

					_idleTimer.Start();
					_opponentAttackStats.ClosingInSpeed = 1;
					_opponentAttackStats.StrafingSpeed = 1;
					_opponentAttackStats.DamageReactMutliplier = 0;
					_opponentAttackStats.KnockReactMutliplier = 0;

					_model.StandardMaterial3D.AlbedoColor = new Color("1D2B53");

					break;


				case "stun":

					Animation = "RESET";

					break;
				

				case "Big Chop":

					state = "big_chop_start";
					break;


				case "3 Hit Combo":

					GD.Randomize();

					var states = new[] { "3_hit_combo_1hit",
						"3_hit_combo_2hit", "3_hit_combo_3hit" };
					
					var hitChance = GD.Randf();

					int hitChoice = hitChance > 0.5f
						? _last3Hit == 1
							? 0
							: 1
						: _last3Hit == 2
							? 1
							: 2;

					_last3Hit = hitChoice;
					state = states[hitChoice];
					
					break;


				case "Around the World":

					state = _strafeRight 
						? "around_the_world_right"
						: "around_the_world_left";

					_opponentAttackStats.StrafingSpeed = 4;

					break;


				case "Big Push":

					state = "big_push";
					break;


				case "Dig Up Jump":

					state = "dig_up_jump_dig_up";
					break;


				case "Jump Back Slam":

					state = "jump_back_slam_jump_back";
					break;


				case "Sumo Advance":

					_sumoCount = 0;
					_sumoLimit = 6;
					state = "sumo_advance_start";
					break;


				case "sumo_advance_next":

					_sumoCount++;

					state = _sumoCount >= _sumoLimit
						? "sumo_advance_end"
						: State == "sumo_advance_right"
							? "sumo_advance_left"
							: "sumo_advance_right";
					break;


				case "The Crab":

					state = "the_crab_start";
					break;
				
				case "the_crab":
					
					float angleToCentre = GetAngleDifference(Position, Rotation.Y,
						Vector3.Zero, 0.2f);
					
					_strafeRight = angleToCentre < 0;
					
					break;


				case "Wastin Time":

					state = "wastin_time_start";

					break;
					

				case "Sumo Pressure":

					_sumoCount = 0;
					_sumoLimit = 10;
					state = "sumo_pressure";
					break;


				case "sumo_pressure_next":

					_sumoCount++;

					state = _sumoCount > _sumoLimit
						? "sumo_pressure_end"
						: State == "sumo_pressure_right"
							? "sumo_pressure_left"
							: "sumo_pressure_right";
					break;


				case "Instant Teleport":

					state = "instant_teleport";
					break;
			}

            _model.StandardMaterial3D.AlbedoColor = new Color("FFA300");
            GD.Print(_model.StandardMaterial3D.AlbedoColor);

			base.State = state;
		}
	}

	protected override string Animation
	{
		get => base.Animation;
		set
		{
			if (_model == null) return;

			base.Animation = value;
			
			if (value != _model.Animation)
			{
				_model.NextAnimationLength = (float)GetAnimationCurrentLength();
				_model.Animation = value;
			}
		}
	}

	// types of attack
	public enum AttackTypes
	{
		CloseToWall, // performed when target is close to wall
		FarFromWall, // performed when I am close to wall
		Neutral, // performed when in neutral position of ring
		MixUp, // can happen at any point
	}

	private Timer _idleTimer;
	
	
	// Node Functions //
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		GD.Randomize();
		
		if (_stats == null)
		{
			_opponentStats = GetNode<Global>("/root/Global").OpponentStats;
			_stats = _opponentStats;
		}
		else
		{
			_opponentStats = _stats as OpponentStats;
		}

		Health = _stats.MaxHealth;
		_opponentAttackStats = AttackStats as OpponentAttackStats;
		
		_idleTimer = GetNode<Timer>("IdleTimer");
		_idleTimer.WaitTime = _opponentStats.IdleDuration;

		_uniqueAttackLabel = GetNode<Label3D>("UniqueAttackLabel");
		_model = GetNode<ActorModel>("Model");

		// set state property now that everything has been loaded
		if (_startState != null)
		{
			State = _startState;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		// do not process anything after this point if there is not target
		if (Target == null) return;


		// get angle difference from centre of ring, which is the origin of the world
		float angleToCentre = GetAngleDifference(Position, Rotation.Y,
			Vector3.Zero, 0.2f);
		
		float angleToCentrePercentage = angleToCentre / Mathf.Pi;

		float distanceFromCentre = Position2D.DistanceTo(Vector2.Zero);
		float distanceFromTarget = 100;

		if (Target is Actor a)
		{
			distanceFromTarget = Position2D.DistanceTo(a.Position2D);
		}


		switch (State)
		{
			case "idle":

				switch (_opponentAttackStats.ClosingInSpeed)
				{
					case > 0.25f:
						Animation = "walk_forward"; break;
					case < -0.25f:
						Animation = "walk_back"; break;
					default:
						Animation = "idle"; break;
				}

				// choose which direction to strafe in
				_strafeRight = angleToCentre < 0;
				
				// initialise as Neutral
				_currentAttackType = AttackTypes.Neutral;
				
				// only change attack type if the following conditions are met
				if (distanceFromCentre > _opponentStats.CloseToWallDistance)
				{
					// must be backed up
					if (angleToCentrePercentage 
					   is < 0.25f 
					   and > -0.25f)
					{
						_currentAttackType = AttackTypes.FarFromWall;
					}
					// must have backed the Player up
					else if (angleToCentrePercentage 
							 is > 0.75f 
							 or < -0.75f)
					{
						_currentAttackType = AttackTypes.CloseToWall;
					}
				}

				_uniqueAttackLabel.Text = _currentAttackType.ToString();

				
				bool canAttack = true;

				// don't attack if player is charging an attack up
				if (Target is Player p)
				{
					canAttack = p.State
						is  not "charge"
						and not "punch_light"
						and not "punch_medium"
						and not "punch_heavy";
				}
				
				// if withing attacking range, attack
				if (Position2D.DistanceTo(Vector3To2D(Target.Position))
					< _opponentStats.ClosingInDistance
					&& canAttack)
				{
					StartAttack();
				}

				break;
			
			case "the_crab":
				
				// if already positioned optimally
				if (angleToCentrePercentage 
					is > 0.9f
					or < -0.9f)
				{
					State = "the_crab_end_start";
				}
				
				break;
		}
		
		Move(Vector2.Down * _opponentAttackStats.ClosingInSpeed);
		Move(Vector2.Right * _opponentAttackStats.StrafingSpeed * (_strafeRight ? 1 : -1));
	}


	// Other Functions //

	private void StartAttack()
	{
		GD.Randomize();

		float attackChance = GD.Randf();

		Array<string> attackList = _opponentStats.CurrentUniqueAttacks[
			_currentAttackType];
		
		Array<string> mixUpList = _opponentStats.CurrentUniqueAttacks[
			AttackTypes.MixUp];

		if (attackList.Count == 0 && mixUpList.Count == 0) return;

		if ((attackChance <= _opponentStats.MixUpChance
			|| attackList.Count == 0)
			&& mixUpList.Count != 0)
		{
			State = mixUpList.PickRandom();
			return;
		}
		
		State = attackList.PickRandom();
	}

	public override void Hurt(float damage, float knock)
	{
		float d = damage;
		float k = knock;

		if (State == "instant_teleport")
		{
			k = 1;
		}

		base.Hurt(d, k);
	}
}
