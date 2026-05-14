shader_type canvas_item;

void fragment() {
    // Obtiene el color original del píxel
    vec4 tex_color = texture(TEXTURE, UV);
    
    // Calcula la escala de grises usando la luminosidad percibida por el ojo humano
    float gray = tex_color.r * 0.299 + tex_color.g * 0.587 + tex_color.b * 0.114;
    
    // Aplica el color gris manteniendo el canal alfa (transparencia) original
    COLOR = vec4(vec3(gray), tex_color.a);
}
