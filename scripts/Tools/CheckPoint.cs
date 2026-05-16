using Godot;
using Faeterna.Scripts.Personaje;
using PlayerType = Faeterna.Scripts.Personaje.Lira;
using System.Threading.Tasks;
using System; // Necesario para Task

namespace Faeterna.Scripts.Tools
{
    public partial class CheckPoint : Node2D
    {
        /// <summary>Identificador único del checkpoint. Se utiliza para diferenciar este checkpoint de otros en el sistema de guardado. </summary>
        [Export] private string _checkpointId = "checkpoint_01";

        /// <summary>Nodo que representa el punto de aparición (spawn point) asociado a este checkpoint. Se asigna desde el editor. Al interactuar con el checkpoint, el jugador se teletransportará a la posición global de este nodo al cargar el progreso guardado. Si no se asigna un spawn point específico, se usará la posición del nodo del checkpoint como punto de aparición por defecto.</summary>
        [Export] private Node2D _spawnPoint;

        /// <summary>
        /// Acción que se ejecuta al interactuar con el checkpoint. Restaura algo de salud y mana al jugador,
        /// y guarda el progreso actual (escena, posición, checkpoint) en el sistema de guardado. Se llama desde el método de interacción del jugador al detectar que se encuentra dentro del área del checkpoint y se presiona la tecla de interacción. Si el juego está pausado, no realiza ninguna acción.
        /// </summary>
        /// <param name="player">
        /// El nodo del jugador (Lira) que interactúa con el checkpoint. Se espera que este nodo tenga métodos para curar salud y recuperar mana, así como para obtener su posición actual. Este parámetro se utiliza para aplicar los efectos de curación y recuperación, y para obtener la posición del jugador al guardar el progreso.
        /// </param>
        public async Task ActionSaveProgress(Lira player)
        {
            if (GetTree().Paused) return;
            player.Heal(5);
            player.RecoverMana(100);
            string scenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
            Vector2 savePosition = _spawnPoint?.GlobalPosition ?? GlobalPosition;

            await GameSaveService.SaveCheckpointAsync(player, _checkpointId, scenePath, savePosition);
            GD.Print($"Checkpoint guardado ({_checkpointId}) mediante interacción.");
        }

        /// <summary>
        /// Devuelve la posición global del punto de aparición asociado a este checkpoint. Si se ha asignado un nodo de spawn point específico, devuelve su posición global; de lo contrario, devuelve la posición global del nodo del checkpoint. Esta posición se utiliza como punto de aparición para el jugador al cargar el progreso guardado desde este checkpoint.
        /// </summary>
        /// <returns>
        /// La posición global del spawn point asociado a este checkpoint, o la posición global del nodo del checkpoint si no se ha asignado un spawn point específico. Esta posición se utiliza para teletransportar al jugador al cargar el progreso guardado desde este checkpoint.
        /// </returns>
        public Vector2 GetPrayPosition() => GlobalPosition;

        /// <summary>
        /// Método que se llama cuando un cuerpo (body) entra en el área de detección del checkpoint. Actualmente no implementa lógica adicional, pero puede ser utilizado en el futuro para detectar la presencia del jugador u otros eventos relacionados con el checkpoint. Se espera que este método sea conectado a la señal "body_entered" del nodo Area2D que representa el área de detección del checkpoint.
        /// </summary>
        /// <param name="body">
        /// El nodo del cuerpo que ha entrado en el área de detección del checkpoint. Este parámetro puede ser utilizado para verificar si el cuerpo es el jugador (Lira) y realizar acciones específicas en respuesta a su presencia, como mostrar un mensaje de interacción o activar efectos visuales. Actualmente no se implementa ninguna lógica dentro de este método, pero se deja preparado para futuras funcionalidades relacionadas con la detección de cuerpos en el área del checkpoint.
        /// </param>
        public void OnAreaCheckPointEntered(Node2D body)
        {
        }
    }
}
