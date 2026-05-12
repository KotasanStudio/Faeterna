using Godot;

namespace Faeterna.scripts.Tools
{
    /// <summary>
    /// Gestor del cursor personalizado del juego. Carga una textura de cursor personalizada desde el sistema de archivos,
    /// la redimensiona a un tamaño específico, y la aplica al juego como el cursor del mouse.
    /// Cuando el nodo se libera, restaura el cursor predeterminado del sistema.
    /// </summary>
    public partial class CursorManager : Node
    {
        /// <summary>Ruta a la textura del cursor personalizado en los assets del proyecto.</summary>
        private const string CursorPath = "res://assets/Icono y Cursor/Cursor.svg";

        /// <summary>Tamaño en píxeles al que se redimensionará el cursor (ancho x alto).</summary>
        private static readonly Vector2I CursorSize = new(32, 32);

        /// <summary>Punto de anclaje (hotspot) del cursor respecto a la textura (esquina superior izquierda).</summary>
        private static readonly Vector2 CursorHotspot = new(2, 2);

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Carga y aplica el cursor personalizado.
        /// </summary>
        public override void _Ready()
        {
            ApplyCustomCursor();
        }

        /// <summary>
        /// Carga la textura del cursor desde el archivo especificado, la redimensiona al tamaño configurado
        /// y la aplica como el cursor del mouse del juego. Si la textura no se puede cargar o procesar, imprime un aviso.
        /// </summary>
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

        /// <summary>
        /// Se llama cuando el nodo sale de la escena. Restaura el cursor predeterminado del sistema.
        /// </summary>
        public override void _ExitTree()
        {
            Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);
        }
    }
}

