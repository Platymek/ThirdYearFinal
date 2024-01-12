using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ForcePalette : CanvasLayer
{
	public ShaderMaterial Filter;

	[Export] Color[] _colors;

	public override void _Ready()
	{
		Filter = (ShaderMaterial)GetNode<ColorRect>("ColorRect").Material;

		//ResetFilter();
	}

/*
	public void ResetFilter()
	{
		Singleton s = GetNode<Singleton>("/root/Singleton");

		Filter.SetShaderParameter("c1", Variant.From(s.Colours1[s.CurrentPalette]));
		Filter.SetShaderParameter("c2", Variant.From(s.Colours2[s.CurrentPalette]));
	}
*/

	public override void _Process(double delta)
	{
		base._Process(delta);

		Filter.SetShaderParameter("colors", Variant.From(_colors));
	}

}
