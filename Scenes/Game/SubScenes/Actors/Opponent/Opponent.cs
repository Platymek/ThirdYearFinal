using Godot;
using System;

public partial class Opponent : Actor
{
	// Properties //

	[Export] private Node _cornersNode;
	
	
	// Node Functions //
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
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

				if (_cornersNode != null)
				{
					Node3D closestCorner;
					float distanceToClosest = 9999;
					
					foreach (Node3D corner in _cornersNode.GetChildren())
					{
						float distanceToTarget = Vector3To2D(Target.Position)
							.DistanceSquaredTo(Vector3To2D(Target.Position));
						
						// if the new corner is closer than the supposed closest, replace
						if (distanceToClosest < distanceToTarget) continue;
						
						closestCorner = corner;
						distanceToClosest = distanceToTarget;
					}
				}
				
				// TODO: Finish off this A.I. stuff!
				
				break;
		}
	}
}
