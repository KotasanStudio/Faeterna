using Godot;
using Faeterna.scripts.Personaje;

namespace Faeterna.scripts.Tools
{
    /// <summary>
    /// Punto de verificación (checkpoint) que guarda el progreso del jugador cuando este entra en su área.
    /// Al entrar en el checkpoint, se guardan los datos del jugador (posición, salud, maná, habilidades)
    /// y se captura una imagen de vista previa de la partida. El jugador respawneará en este punto si muere o carga la partida.
    /// </summary>
    public partial class CheckPoint : Node2D
    {
        /// <summary>Identificador único del checkpoint para rastrear qué punto de control fue alcanzado por último.</summary>
        [Export] private string _checkpointId = "checkpoint_01";

        /// <summary>Punto de respawn del jugador cuando carga desde este checkpoint. Si es nulo, se usa la posición del checkpoint.</summary>
        [Export] private Node2D _spawnPoint;

        /// <summary>
        /// Se llama cuando el cuerpo del jugador entra en el área del checkpoint. Guarda el progreso y genera una imagen de vista previa.
        /// Solo funciona si el juego no está pausado y el cuerpo que entra es el jugador (Lira).
        /// </summary>
        /// <param name="body">El nodo que entró en el área del checkpoint (generalmente el jugador).</param>
        public async void OnAreaCheckPointEntered(Node2D body)
        {
            if (GetTree().Paused)
            {
                return;
            }

            if (body is not Lira player)
            {
                return;
            }

            string scenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
            Vector2 savePosition = _spawnPoint?.GlobalPosition ?? GlobalPosition;

            await GameSaveService.SaveCheckpointAsync(player, _checkpointId, scenePath, savePosition);
            GD.Print($"Checkpoint guardado ({_checkpointId}) en slot {GameSaveService.ActiveSlot + 1}.");
        }
    }
}

