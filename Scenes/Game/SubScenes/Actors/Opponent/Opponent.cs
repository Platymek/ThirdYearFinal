using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using static Godot.WebSocketPeer;

public partial class Opponent : Actor
{
	// Properties //

	[Export] private string _startState;
	
	private OpponentStats _opponentStats;
	private OpponentAttackStats _opponentAttackStats;
	private Label3D _uniqueAttackLabel;
	private ActorModel _model;
	private AttackTypes _currentAttackType;

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
					
					if (State == "stun")
					{
						stunEnded = true;
                    }

					break;

                case "3 Hit Combo":

                    GD.Randomize();

					string[] states = new string[] { "3_hit_combo_1hit", 
						"3_hit_combo_2hit", "3_hit_combo_3hit" };

                    state = states[GD.RandRange(0, states.Length)];

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

		switch (State)
		{
			case "idle":
				
				_opponentAttackStats.ClosingInSpeed = 1;

				// get angle difference from centre of ring, which is the origin of the world
				float angleToTarget = GetAngleDifference(Target.Position, Rotation.Y,
					Vector3.Zero, 0.2f);

				float distanceFromCentre = Position2D.DistanceTo(Vector2.Zero);

				switch (_opponentAttackStats.ClosingInSpeed)
				{
					case > 0.25f:
						Animation = "walk_forward"; break;
					case < -0.25f:
						Animation = "walk_back"; break;
					default:
						Animation = "idle"; break;
				}

				// try to move with back turned to center of ring if a certain distance away from ring
				if (distanceFromCentre > _opponentStats.CenterStrafingDistance)
				{
					switch (angleToTarget)
					{
						case > 0:
                            _opponentAttackStats.StrafingSpeed = -1;
							break;
						case < 0:
                            _opponentAttackStats.StrafingSpeed = 1;
                            break;
					}
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
        Move(Vector2.Right * _opponentAttackStats.StrafingSpeed);
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
