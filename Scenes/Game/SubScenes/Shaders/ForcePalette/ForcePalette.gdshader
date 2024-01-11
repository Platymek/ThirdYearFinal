shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, 
	filter_nearest;

uniform vec4[16] colors;

void fragment() {
	
	vec4 screenColor = textureLod(screen_texture, SCREEN_UV, 0.0);
	
	int closestColor = 0;
	float closestDistance = distance(screenColor, colors[0]);
	
	for (int i = 0; i < 16; i++) {
		
		float currentDistance = distance(screenColor, colors[i]);
		
		if (currentDistance < closestDistance) {
			
			closestColor = i;
			closestDistance = currentDistance;
		}
	}
	
    COLOR *= colors[closestColor];
}