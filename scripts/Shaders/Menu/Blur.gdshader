shader_type canvas_item;

// Controla qué tan borroso se ve (de 0.0 a 5.0)
uniform float intensidad : hint_range(0.0, 5.0) = 2.0;

void fragment() {
	// textureLod usa los 'mipmaps' de la imagen para crear el desenfoque
	COLOR = textureLod(TEXTURE, UV, intensidad);
}
