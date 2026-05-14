shader_type canvas_item;

// Controla el progreso de la desaparición (0.0 = Visible, 1.0 = Desaparecido)
uniform float progreso : hint_range(0.0, 1.0) = 0.0;

// Textura de ruido que define el patrón de desaparición
uniform sampler2D textura_ruido : filter_nearest;

// Color del borde brillante del efecto
uniform vec4 color_borde : source_color = vec4(1.0, 0.3, 0.0, 1.0);
uniform float grosor_borde : hint_range(0.0, 0.2) = 0.05;

void fragment() {
    // Color y transparencia original del sprite
    vec4 color_original = texture(TEXTURE, UV);
    
    // Lee el valor en escala de grises del ruido en este píxel
    float valor_ruido = texture(textura_ruido, UV).r;
    
    // Si el valor del ruido es menor al progreso actual, el píxel se descarta
    if (valor_ruido < progreso) {
        discard;
    }
    
    // Aplica el color del borde brillante justo antes de que el píxel se descarte
    if (valor_ruido < progreso + grosor_borde) {
        COLOR = color_borde;
    } else {
        COLOR = color_original;
    }
    
    // Mantiene la transparencia original de los bordes del sprite
    COLOR.a *= color_original.a;
}