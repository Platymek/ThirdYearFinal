using Godot;
using System;

public partial class Opponent : Actor
{
	// Properties //
	
	private OpponentStats _opponentStats;
	private OpponentAttackStats _opponentAttackStats;
	private Label3D _uniqueAttackLabel;
	private ActorModel _model;

	public override string State
	{
		get => base.State;
		
		protected set
		{
			base.State = value;
			
			_opponentAttackStats.ClosingInSpeed = 1;
		}
	}

    protected override string Animation
    {
        get => base.Animation;
        set
        {
            if (_model == null) return;

			if (value != _model.Animation)
            {
                _model.NextAnimationLength = (float)GetAnimationCurrentLength();
                _model.Animation = value;
            }

            base.Animation = value;
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

				bool closedIn = Position2D.DistanceTo(Vector3To2D(Target.Position))
					< _opponentStats.ClosingInDistance;

				// get angle difference from centre of ring, which is the origin of the world
				float angleToTarget = GetAngleDifference(Target.Position, Rotation.Y,
					Vector3.Zero, 0.1f);

				float distanceFromCentre = Position2D.DistanceTo(Vector2.Zero);

				// set animation when closing in
                switch (_opponentAttackStats.ClosingInSpeed)
                {
                    case > 0.25f:
                        Animation = "walk_forward"; break;
                    case < 0.25f:
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
                            Animation = "walk_left";
                            Move(Vector2.Left * (closedIn ? 1 : .25f));
							break;
						case < 0:
                            Animation = "walk_right";
                            Move(Vector2.Right * (closedIn ? 1 : .25f));
							break;
					}
				}
					
				if (closedIn)
				{
					_opponentAttackStats.ClosingInSpeed = .25f;
				}


				_uniqueAttackLabel.Text = "Neutral";

				if (distanceFromCentre > _opponentStats.CloseToWallDistance)
				{
					if (angleToTarget < Math.PI * 0.25f | angleToTarget > Math.PI * -0.25f)
					{
						_uniqueAttackLabel.Text = "CloseToWall";
					}
					else if (angleToTarget > Math.PI * 0.75f | angleToTarget < Math.PI * -0.75f)
					{
						_uniqueAttackLabel.Text = "FarFromWall";
					}
				}
				
				break;
		}
		
		Move(Vector2.Down * _opponentAttackStats.ClosingInSpeed);
	}
}
