using Godot;
using System;

public partial class Crowd : Node3D
{
	[Export] private AnimationPlayer _animationPlayer;

	public void Cheer()
	{
		_animationPlayer.Play("cheer");
	}
}
