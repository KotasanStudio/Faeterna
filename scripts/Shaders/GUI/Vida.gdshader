shader_type canvas_item;

uniform float hit_effect : hint_range(0.0, 1.0) = 0.0; 
uniform float time_scale = 10.0;
uniform bool its_on = true;

void fragment() {
	if (!its_on){
		vec4 tex = texture(TEXTURE, UV);

	    // Convertir a escala de grises
	    float gray = dot(tex.rgb, vec3(0.299, 0.587, 0.114));

	    // Efecto pulso
	    float pulse = sin(TIME * time_scale) * 0.5 + 0.5;

	    // Mezclar color original con versión vacía
	    vec3 void_color = vec3(gray) * 0.2; // más oscuro

	    vec3 final_color = mix(tex.rgb, void_color + pulse * 0.2, hit_effect);

	    COLOR = vec4(final_color, tex.a);
	}
}
