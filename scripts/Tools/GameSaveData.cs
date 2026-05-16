using Godot;
using System.Text.Json.Serialization;

namespace Faeterna.Scripts.Tools
{
    /// <summary>
    /// Clase que contiene los datos completos de una partida guardada, incluyendo información de la escena actual,
    /// datos del jugador y puntos de control. Se utiliza para serializar y deserializar el estado del juego desde el almacenamiento persistente.
    /// </summary>
    public class GameData
    {
        /// <summary>Número del espacio de guardado (slot) donde se almacenan estos datos.</summary>
        public int SaveSlot { get; set; }

        /// <summary>Ruta de la escena donde el jugador estaba cuando se guardó la partida.</summary>
        public string ScenePath { get; set; } = string.Empty;

        /// <summary>Identificador único del último punto de control (checkpoint) alcanzado por el jugador.</summary>
        public string LastCheckpointId { get; set; } = string.Empty;

        /// <summary>Ruta del archivo de imagen de vista previa que representa el estado guardado en la interfaz de usuario.</summary>
        public string PreviewImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Datos específicos del jugador (posición, salud, maná, habilidades adquiridas y estado de tutoriales).
        /// Se serializan junto con la partida para restaurar correctamente a Lira al cargar el guardado.
        /// </summary>
        public PlayerSaveData PlayerData { get; set; } = new();

        /// <summary>
        /// Lista de identificadores de bosses derrotados en esta partida.
        /// Se usa para que esos bosses no reaparezcan al cargar el guardado.
        /// </summary>
        public System.Collections.Generic.List<string> DefeatedBossTypes { get; set; } = new();
    }

    /// <summary>
    /// Clase que contiene los datos específicos del jugador (Lira) que se guardan en una partida.
    /// Incluye información sobre la posición, salud, maná y disponibilidad de habilidades especiales como el doble salto, dash y coyote time.
    /// </summary>
    public class PlayerSaveData
    {
        /// <summary>Componente X de la posición del jugador en el mundo.</summary>
        public float PositionX { get; set; }

        /// <summary>Componente Y de la posición del jugador en el mundo.</summary>
        public float PositionY { get; set; }

        /// <summary>Puntos de vida actuales del jugador. El valor predeterminado es 5 (máximo).</summary>
        public int Health { get; set; } = 5;

        /// <summary>Maná actual del jugador. El valor predeterminado es 100f (máximo).</summary>
        public float Mana { get; set; } = 100f;

        /// <summary>Indica si el doble salto está disponible para ser usado. El valor predeterminado es verdadero.</summary>
        public bool DoubleJumpAvailable { get; set; } = true;

        /// <summary>Indica si el dash está disponible para ser usado. El valor predeterminado es verdadero.</summary>
        public bool DashAvailable { get; set; } = true;

        /// <summary>Indica si el coyote time está disponible (permite saltar brevemente tras abandonar el suelo). El valor predeterminado es verdadero.</summary>
        public bool CoyoteAvailable { get; set; } = true;

        /// <summary>Indica si el jugador ha adquirido el doble salto como habilidad permanente. El valor predeterminado es falso (no adquirido).</summary>
        public bool HasDoubleJump { get; set; } = false;

        /// <summary>Indica si el jugador ha adquirido el dash como habilidad permanente. El valor predeterminado es falso (no adquirido).</summary>
        public bool HasDash { get; set; } = false;

        /// <summary>
        /// Indica si el jugador ya vio o completó los tutoriales relevantes.
        /// Se usa para no volver a mostrar tutoriales tras cargar la partida.
        /// </summary>
        public bool HasCompletedTutorial { get; set; } = false;

        /// <summary>
        /// Posición del jugador como un <see cref="Vector2"/>. Esta propiedad convierte automáticamente entre componentes X,Y y Vector2 para facilitar el acceso a la posición completa del jugador.
        /// No se serializa en JSON para mantener la compatibilidad con almacenamiento persistente.
        /// </summary>
        [JsonIgnore]
        public Vector2 Position
        {
            get => new(PositionX, PositionY);
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }
    }
}
