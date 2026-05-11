shader_type canvas_item;

uniform float fill_amount : hint_range(0.0, 1.0) = 1.0;
uniform float damage_flash : hint_range(0.0, 1.0) = 0.0;

void fragment()
{
    vec4 tex = texture(TEXTURE, UV);

    // Escala de grises para estado vacío
    float gray = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    vec3 empty_color = vec3(gray) * 0.25;

    // Mezcla lleno/vacío permanente
    vec3 base_color = mix(empty_color, tex.rgb, fill_amount);

    // Flash blanco temporal
    vec3 flash_color = mix(base_color, vec3(1.0), damage_flash);

    COLOR = vec4(flash_color, tex.a);
}