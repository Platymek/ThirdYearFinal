using Godot;
using System;

public partial class Opponent : Actor
{
	// Properties //
	
	private OpponentStats _opponentStats;
	private OpponentAttackStats _opponentAttackStats;

	[Export] private Node _cornersNode;

	public override string State
	{
		get => base.State;
		
		protected set
		{
			base.State = value;
			
			_opponentAttackStats.ClosingInSpeed = 1;
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
		
		_opponentStats = _stats as OpponentStats;
		_opponentAttackStats = AttackStats as OpponentAttackStats;
		
		_idleTimer = GetNode<Timer>("IdleTimer");
		_idleTimer.WaitTime = _opponentStats.IdleDuration;
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

				/*
				if (_cornersNode != null)
				{
					Node3D closestCorner = _cornersNode.GetChild<Node3D>(0);
					float distanceToClosest = 9999;
					
					// find the closest corner
					foreach (Node3D corner in _cornersNode.GetChildren())
					{
						float distanceToTarget = Target.Position
							.DistanceSquaredTo(corner.Position);
						
						// if the new corner is closer than the supposed closest, replace
						if (distanceToClosest < distanceToTarget) continue;
					
						GD.Print(closestCorner.Position);
						
						closestCorner = corner;
						distanceToClosest = distanceToTarget;
					}
					
					// get angle difference (the smallest angle to the corner)

					float angleToTarget = GetAngleDifference(Target.Position, Rotation.Y,
						closestCorner.Position, 0.1f);

					switch (angleToTarget)
					{
						case > 0:
							Move(Vector2.Right);
							break;
						case < 0:
							Move(Vector2.Left);
							break;
					}
				}
				*/
				
				_opponentAttackStats.ClosingInSpeed = 1;

				bool closedIn = Position2D.DistanceTo(Vector3To2D(Target.Position))
					< _opponentStats.ClosingInDistance;

				// get angle difference from centre of ring, which is the origin of the world
				float angleToTarget = GetAngleDifference(Target.Position, Rotation.Y,
					Vector3.Zero, 0.1f);

				// try to move with back turned to center of ring if a certain distance away from ring
				if (Position2D.DistanceTo(Vector2.Zero) > _opponentStats.CenterStrafingDistance)
				{
					switch (angleToTarget)
					{
						case > 0:
							Move(Vector2.Left * (closedIn ? 1 : .25f));
							break;
						case < 0:
							Move(Vector2.Right);
							break;
					}
				}
					
				if (closedIn)
				{
					_opponentAttackStats.ClosingInSpeed = .25f;
				}
				
				break;
		}
		
		Move(Vector2.Down * _opponentAttackStats.ClosingInSpeed);
	}
}
