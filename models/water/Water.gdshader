shader_type spatial;
uniform vec3 baseColor : source_color = vec3(1.0);
uniform sampler2D tex0;
uniform sampler2D tex1;
uniform vec2 scroll0;
uniform vec2 scroll1;
uniform float rot0 = 0;
uniform float rot1 = 0;

varying float heightMult;

varying float posZ;
void vertex() {
	// Called for every vertex the material is visible on.
	
	posZ = VERTEX.z;
	float orig = VERTEX.y;
	VERTEX += vec3(0, 0.12*sin(0.5*TIME*VERTEX.x) + 0.12*sin(0.2*TIME*VERTEX.z), 0);
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 uv0_ur = UV + (TIME*scroll0);
	vec2 uv0 = vec2(uv0_ur.x*cos(rot0)-uv0_ur.y*sin(rot0), uv0_ur.y*cos(rot0)+uv0_ur.x*sin(rot0));
	vec2 uv1_ur = UV + (TIME*scroll1);
	vec2 uv1 = vec2(uv1_ur.x*cos(rot1)-uv1_ur.y*sin(rot1), uv1_ur.y*cos(rot1)+uv1_ur.x*sin(rot1));
	vec3 t0 = texture(tex0,uv0*50.0).rgb;
	vec3 t1 = texture(tex1,uv1*50.0).rgb;

	vec3 p1 = (t0*t1);
	vec3 p2 = ((baseColor - p1)*vec3(0.5,0.5,0.5))+p1;
	//vec3 color = vec3(t0.r+t1.r, t0.g+t1.g, t0.b+t1.b);
	//ALBEDO = baseColor * color;
	
	float lerpVal = clamp(posZ*-0.15, 0.02, 1.0);
	vec3 finalColor = mix(p2, p2*vec3(0, 0, 1), lerpVal);
	ALBEDO = finalColor;


	SPECULAR = 1.0;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
