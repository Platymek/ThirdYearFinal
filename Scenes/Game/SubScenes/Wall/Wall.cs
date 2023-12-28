using Godot;
using System;
using System.Collections.Generic;

public partial class Wall : StaticBody3D
{
	private List<Actor> _collidingActors;

	public override void _Ready()
	{
		base._Ready();

		_collidingActors = new List<Actor>();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		List<Actor> collidedActors = new List<Actor>();

		foreach (var actor in _collidingActors)
		{
			if (actor.WallBounce(Rotation.Y))
			{
				collidedActors.Add(actor);
			}
		}
		
		foreach (var actor in collidedActors)
		{
			_collidingActors.Remove(actor);
			GD.Print($"{actor.Name} stopped Colliding!");
		}
	}

	private void OnWallEntered(Area3D area)
	{
		if (area.Owner is not Actor actor) return;
		
		_collidingActors.Add(actor);
		GD.Print($"{actor.Name} started Colliding!");
	}

	private void OnWallExited(Area3D area)
	{
		if (area.Owner is not Actor actor) return;
		
		_collidingActors.Remove(actor);
		GD.Print($"{actor.Name} stopped Colliding!");
	}
}
