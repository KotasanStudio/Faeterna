shader_type canvas_item;

uniform float fill_amount : hint_range(0.0, 1.0) = 1.0;
uniform vec4 liquid_color : source_color = vec4(0.5, 0.8, 1.0, 1.0);
uniform vec4 empty_color : source_color = vec4(0.1, 0.1, 0.15, 1.0);

uniform float wave_speed = 2.5;
uniform float wave_strength = 0.015;
uniform float wave_frequency = 12.0;
uniform float highlight_strength = 0.4;

void fragment()
{
    vec2 uv = UV;
    
    // Centrar la esfera
    vec2 centered = uv - vec2(0.5);
    
    // Corregir si el rectángulo no es cuadrado
    float aspect = TEXTURE_PIXEL_SIZE.x / TEXTURE_PIXEL_SIZE.y;
    centered.x /= aspect;

    float radius = length(centered);

    // Mantener la base visible siempre
    if(radius > 0.5)
        discard;

    vec3 base = empty_color.rgb;

    if (fill_amount > 0.0)
    {
        // Ondas solo si no está lleno ni vacío
        float wave = 0.0;
        if(fill_amount > 0.0 && fill_amount < 1.0)
        {
            float wave1 = sin(uv.x * wave_frequency + TIME * wave_speed);
            float wave2 = sin(uv.x * (wave_frequency*0.6) - TIME * wave_speed*0.7);
            wave = (wave1 + wave2) * 0.5 * wave_strength;
        }

        float liquid_height = 1.0 - fill_amount + wave;

        if (uv.y > liquid_height)
        {
            base = liquid_color.rgb;

            // Gradiente interno para volumen
            float vertical_gradient = smoothstep(0.0, 1.0, uv.y);
            base *= mix(0.9, 1.1, vertical_gradient);

            // Brillo en superficie
            float edge = smoothstep(liquid_height, liquid_height - 0.01, uv.y);
            base += edge * highlight_strength;

            // Iluminación tipo esfera
            float sphere_light = 1.0 - radius*1.5;
            base += sphere_light * 0.1;
        }
    }

    COLOR = vec4(base, 1.0);
}