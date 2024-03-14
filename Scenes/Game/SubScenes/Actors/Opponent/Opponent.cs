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
	private bool strafeRight = false;

	public override string State
	{
		get => base.State;
		
		protected set
		{
			if (_opponentAttackStats == null) return;

            _opponentAttackStats.ClosingInSpeed = 0;
            _opponentAttackStats.StrafingSpeed = 0;

			bool stunEnded = false;
			string state = value;

            switch (value)
			{
				case "idle":

					_idleTimer.Start();
					_opponentAttackStats.ClosingInSpeed = 1;
                    _opponentAttackStats.StrafingSpeed = 1;

                    if (State == "stun")
					{
						stunEnded = true;
                    }

					break;
				
				case "Big Chop":

					state = "big_chop_start";
					break;

                case "3 Hit Combo":

                    GD.Randomize();

                    string[] states = new string[] { "3_hit_combo_1hit",
                        "3_hit_combo_2hit", "3_hit_combo_3hit" };

                    state = states[GD.RandRange(0, states.Length)];

                    break;

                case "Around the World":

					if (strafeRight)
					{
						state = "around_the_world_right";
					}
					else
					{
						state = "around_the_world_left";
					}

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
            }

			base.State = state;

			if (stunEnded)
			{
				Animation = "stun_end";
			}
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
        float angleToTarget = GetAngleDifference(Target.Position, Rotation.Y,
            Vector3.Zero, 0.2f);

        float distanceFromCentre = Position2D.DistanceTo(Vector2.Zero);
		float distanceFromTarget = 100;

        if (Target is Actor a)
        {
            distanceFromTarget = Position2D.DistanceTo(a.Position2D);
        }

        strafeRight = angleToTarget < 0;


        switch (State)
		{
			case "idle":

				if (Target is Player p)
                {
					_opponentAttackStats.ClosingInSpeed =
						p.State == "charge" || p.State == "punch_medium" || p.State == "punch_heavy"
                        ? (distanceFromTarget > 2 
							? 0 
							: -1)
						: 1;
                }

				switch (_opponentAttackStats.ClosingInSpeed)
				{
					case > 0.25f:
						Animation = "walk_forward"; break;
					case < -0.25f:
						Animation = "walk_back"; break;
					default:
						Animation = "idle"; break;
				}


                _currentAttackType = AttackTypes.Neutral;

                if (distanceFromCentre > _opponentStats.CloseToWallDistance)
				{
					if (angleToTarget < Math.PI * 0.25f | angleToTarget > Math.PI * -0.25f)
					{
                        _currentAttackType = AttackTypes.CloseToWall;
					}
					else if (angleToTarget > Math.PI * 0.75f | angleToTarget < Math.PI * -0.75f)
                    {
                        _currentAttackType = AttackTypes.FarFromWall;
                    }
				}

                _uniqueAttackLabel.Text = _currentAttackType.ToString();


				// if withing attacking range, attack
                if (Position2D.DistanceTo(Vector3To2D(Target.Position))
					< _opponentStats.ClosingInDistance)
				{
					StartAttack();
                }

                break;
		}
		
		Move(Vector2.Down * _opponentAttackStats.ClosingInSpeed);
        Move(Vector2.Right * _opponentAttackStats.StrafingSpeed * (strafeRight ? 1 : -1));
    }


	// Other Functions //

	private void StartAttack()
	{
		GD.Randomize();

		float attackChance = GD.Randf();

		Array<string> attackList = _opponentStats.CurrentUniqueAttacks[
			_currentAttackType];

		bool mixUpChosen = attackChance >= 0.5f || attackList.Count == 0;


        if (mixUpChosen)
		{
			State = _opponentStats.MixUpAttacks.PickRandom();
        }
		else
        {
            State = attackList.PickRandom();
        }
	}
}
