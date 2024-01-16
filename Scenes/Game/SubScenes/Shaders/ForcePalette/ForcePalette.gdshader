shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, 
	filter_nearest;

uniform vec4[16] colors;
uniform float brightness;
uniform vec4 tint = vec4(0.0);

void fragment() {
	
	vec4 screenColor = textureLod(screen_texture, SCREEN_UV, 0.0)
		* vec4(brightness) + tint;
	
	int closestColor = 0;
	float closestDistance = distance(screenColor, colors[0]);
	
	int secondClosestColor = 0;
	float secondClosestDistance = closestDistance;
	
	for (int i = 0; i < 16; i++) {
		
		float currentDistance = distance(screenColor, colors[i]);
		
		if (currentDistance < closestDistance) {
			
			secondClosestColor = closestColor;
			secondClosestDistance = closestDistance;
			
			closestColor = i;
			closestDistance = currentDistance;
		}
	}
	
	vec4 color = colors[closestColor];
	
    COLOR *= color;
}