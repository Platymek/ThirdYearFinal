using Godot;
using System;

public partial class Wall : Area3D
{
	private void _on_area_entered(Area3D area)
	{
		if (area.Owner is Actor actor)
		{
			GD.Print("WallBounced");

			//actor.WallBounce();
		}
	}
}
