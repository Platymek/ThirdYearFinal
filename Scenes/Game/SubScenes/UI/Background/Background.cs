using Godot;
using System;

public partial class Background : Control
{
	[Export] public Vector2 CrowdPosition
	{
		get => _crowdTexture.Position;
		set => _crowdTexture.Position
				= new Vector2(
					value.X % 360,
					value.Y % 360);
	}

	[ExportGroup("Nodes")]
	[Export] Control _crowdTexture;
}
