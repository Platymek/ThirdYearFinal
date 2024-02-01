shader_type canvas_item;

uniform vec4 color1 : source_color;
uniform vec4 color2 : source_color;

void fragment() {
	
	float waveHeight = 0.;
	
	if (UV.y > .5)
	{
		waveHeight = sin(TIME * .5 + UV.x * 10. - UV.y * 10.);
	}
	else
	{
		waveHeight = cos(TIME * .5 + (UV.x + .1) * 10. + (UV.y) * 10.);
	}
	
	if (waveHeight < 0.)
	
		COLOR = color1;
	else
		COLOR = color2;
}
