using Godot;

namespace Faeterna.scripts.Tools
{
    public partial class CursorManager : Node
    {
        private const string CursorPath = "res://assets/Icono y Cursor/Cursor.svg";
        private static readonly Vector2I CursorSize = new(32, 32);
        private static readonly Vector2 CursorHotspot = new(2, 2);

        public override void _Ready()
        {
            ApplyCustomCursor();
        }

        private static void ApplyCustomCursor()
        {
            var sourceTexture = ResourceLoader.Load<Texture2D>(CursorPath);
            if (sourceTexture == null)
            {
                GD.PushWarning($"No se pudo cargar el cursor: {CursorPath}");
                return;
            }

            var image = sourceTexture.GetImage();
            if (image == null || image.IsEmpty())
            {
                GD.PushWarning("No se pudo obtener la imagen del cursor para redimensionar.");
                return;
            }

            image.Resize(CursorSize.X, CursorSize.Y, Image.Interpolation.Lanczos);
            var scaledTexture = ImageTexture.CreateFromImage(image);
            Input.SetCustomMouseCursor(scaledTexture, Input.CursorShape.Arrow, CursorHotspot);
        }

        public override void _ExitTree()
        {
            Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);
        }
    }
}

