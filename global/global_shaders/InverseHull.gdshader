shader_type spatial;
render_mode cull_front, unshaded;

uniform float outlineWidth;
uniform vec3 outlineAlbedo : source_color;

void vertex() {
	// Called for every vertex the material is visible on.
	VERTEX *= outlineWidth < 1.0 ? 0.0 : outlineWidth;
}

void fragment() {
	// Called for every pixel the material is visible on.
	ALBEDO = outlineAlbedo;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
