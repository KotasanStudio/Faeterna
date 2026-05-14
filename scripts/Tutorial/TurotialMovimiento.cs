using Godot;
using Faeterna.Scripts.Personaje;

namespace Faeterna.Scripts.Tutorial
{
    public partial class TurotialMovimiento : Node2D
    {
        private Area2D _area2D;
        private ShaderMaterial _shaderMaterial;
        private Sprite2D _shaderOverlay;
        private bool _passed;

        public override void _Ready()
        {
            _area2D = GetNodeOrNull<Area2D>("Area2D");

            // Crear un overlay sprite que renderice el shader
            _shaderOverlay = new Sprite2D();
            _shaderOverlay.Centered = true;

            // Crear una imagen blanca para el overlay
            var image = Image.Create(1000, 1000, false, Image.Format.Rgba8);
            image.Fill(Colors.White);
            var texture = ImageTexture.CreateFromImage(image);

            _shaderOverlay.Texture = texture;
            _shaderOverlay.Scale = Vector2.One;

            // Asignar el shader
            var shader = GD.Load<Shader>("res://scripts/Shaders/Tutorial/Desaparecer.gdshader");
            if (shader != null)
            {
                _shaderMaterial = new ShaderMaterial
                {
                    Shader = shader
                };
                _shaderMaterial.SetShaderParameter("progreso", 0.0f);
                _shaderMaterial.SetShaderParameter("color_borde", new Color(1, 0.3f, 0, 1));
                _shaderMaterial.SetShaderParameter("grosor_borde", 0.05f);
                _shaderOverlay.Material = _shaderMaterial;
            }

            AddChild(_shaderOverlay);
            _shaderOverlay.Visible = false; // Mantener invisible hasta que se necesite
            MoveChild(_shaderOverlay, 0); // Ponerlo al frente

            if (_area2D != null)
            {
                _area2D.BodyEntered += OnArea2dAreaEntered;
            }
        }

        public void DesvanecerPersonaje()
        {
            if (_shaderMaterial == null || _passed)
            {
                return;
            }

            _passed = true;
            _shaderOverlay.Visible = true; // Hacer visible para el efecto de desaparición

            Tween tween = CreateTween();

            tween.TweenProperty(
                _shaderMaterial,
                "shader_parameter/progreso",
                1.0f,
                1.5f
            );

            tween.Finished += QueueFree;
        }

        public void OnArea2dAreaEntered(Node2D body)
        {
            if (body is not Lira)
            {
                return;
            }

            DesvanecerPersonaje();
        }
    }
}
