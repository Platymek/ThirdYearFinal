using Godot;
using System;

public partial class OpponentModel : ActorModel
{
	public ShaderMaterial ShaderMaterial;

	public override void _Ready()
	{
		base._Ready();

		ShaderMaterial = GetNode<MeshInstance3D>(
			"Armature/Skeleton3D/ChestConnector")
			.GetSurfaceOverrideMaterial(0)
			.NextPass
			.NextPass
			as ShaderMaterial;
	}
}
