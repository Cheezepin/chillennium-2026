shader_type canvas_item;

uniform sampler2D screenTex : hint_screen_texture;
uniform float fadeOutVal : hint_range(0.0, 1.0, 0.01);
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 col = texture(screenTex, UV);
	COLOR = mix(col, vec4(0,0,0,1), fadeOutVal);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
