using Faeterna.Scripts.Personaje;
using Godot;
using System.Threading.Tasks;
using PlayerType = Faeterna.Scripts.Personaje.Lira;


namespace Faeterna.Scripts.Tools
{
    /// <summary>
    /// Fachada de compatibilidad. La lógica real de guardado está centralizada en <see cref="Saves"/>.
    /// </summary>
    public static class GameSaveService
    {
        public static int ActiveSlot => Saves.ActiveSlot;
        /// <summary>
        /// Datos de juego pendientes de aplicar tras cargar/recargar una escena.
        /// Se usan para almacenar temporalmente el GameData mientras se realiza ChangeScene
        /// y poder aplicarlo al nuevo nodo jugador cuando la escena se haya cargado.
        /// </summary>
        public static GameData PendingGameData { get; private set; }

        /// <summary>
        /// Establece datos pendientes que serán aplicados tras completar el cambio de escena.
        /// </summary>
        /// <param name="data">GameData a aplicar después de recargar la escena.</param>
        public static void SetPendingGameData(GameData data) => PendingGameData = data;

        /// <summary>
        /// Recupera y limpia los datos pendientes.
        /// </summary>
        /// <returns>GameData previamente pendiente o null.</returns>
        public static GameData RetrieveAndClearPending()
        {
            var tmp = PendingGameData;
            PendingGameData = null;
            return tmp;
        }

        // Conjunto de bosses derrotados durante la sesión; se persiste en los guardados.
        private static readonly System.Collections.Generic.HashSet<string> _defeatedBosses = new();

        /// <summary>
        /// Marca un boss como derrotado (persistible en el siguiente guardado).
        /// </summary>
        /// <param name="bossType">Identificador del boss (por ejemplo el nombre de la clase).</param>
        public static void MarkBossDefeated(string bossType)
        {
            if (string.IsNullOrWhiteSpace(bossType)) return;
            _defeatedBosses.Add(bossType);
        }

        /// <summary>
        /// Devuelve una lista copiada de los bosses derrotados actualmente registrados.
        /// </summary>
        /// <returns>Lista de identificadores de bosses derrotados.</returns>
        public static System.Collections.Generic.List<string> GetDefeatedBossList()
            => new System.Collections.Generic.List<string>(_defeatedBosses);

        /// <summary>
        /// Reemplaza la lista de bosses derrotados (usado al cargar un guardado).
        /// </summary>
        public static void SetDefeatedBosses(System.Collections.Generic.IEnumerable<string> list)
        {
            _defeatedBosses.Clear();
            if (list == null) return;
            foreach (var s in list)
                if (!string.IsNullOrWhiteSpace(s)) _defeatedBosses.Add(s);
        }

        /// <summary>
        /// Define el slot activo asegurando que no sea negativo.
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        public static void SetActiveSlot(int slot) => Saves.SetActiveSlot(slot);

        /// <summary>
        /// Indica si existe archivo de guardado para el slot dado.
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        /// <returns><see langword="true"/> si existe el JSON del slot; en caso contrario, <see langword="false"/>.</returns>
        public static bool HasSave(int slot) => Saves.HasSave(slot);

        /// <summary>
        /// Indica si el slot activo tiene archivo de guardado.
        /// </summary>
        /// <returns><see langword="true"/> si existe guardado en el slot activo.</returns>
        public static bool HasSaveInActiveSlot() => Saves.HasSaveInActiveSlot();

        /// <summary>
        /// Indica si existe una imagen preview asociada al slot.
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        /// <returns><see langword="true"/> si existe la imagen PNG del slot.</returns>
        public static bool HasPreview(int slot) => Saves.HasPreview(slot);

        /// <summary>
        /// Carga la textura de preview del slot desde disco.
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        /// <returns>
        /// Una <see cref="Texture2D"/> lista para usar en UI, o <see langword="null"/> si no existe/esta corrupta.
        /// </returns>
        public static Texture2D LoadPreviewTexture(int slot) => Saves.LoadPreviewTexture(slot);

        /// <summary>
        /// Elimina el guardado completo de un slot (datos JSON + preview PNG).
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        public static void DeleteSlot(int slot) => Saves.DeleteSlot(slot);

        /// <summary>
        /// Guarda el progreso al tocar un checkpoint y genera la imagen preview del slot activo.
        /// </summary>
        /// <param name="player">Instancia del jugador desde la que se extrae el estado.</param>
        /// <param name="checkpointId">Identificador del checkpoint alcanzado.</param>
        /// <param name="scenePath">Ruta de escena actual para validar cargas posteriores.</param>
        /// <param name="checkpointPosition">Posicion de respawn a persistir.</param>
        /// <param name="viewport">Viewport fuente para la captura; si es <see langword="null"/>, usa el del jugador.</param>
        /// <returns>Tarea asincrona de guardado.</returns>
        public static Task SaveCheckpointAsync(PlayerType player, string checkpointId, string scenePath, Vector2 checkpointPosition, Viewport viewport = null)
            => Saves.SaveCheckpointAsync(player, checkpointId, scenePath, checkpointPosition, viewport);

        /// <summary>
        /// Carga los datos del slot activo.
        /// </summary>
        /// <returns>Datos cargados o <see langword="null"/> si el archivo no existe.</returns>
        public static async Task<GameData> LoadActiveSlotAsync()
        {
            var data = await Saves.LoadActiveSlotAsync();
            if (data?.DefeatedBossTypes != null)
            {
                SetDefeatedBosses(data.DefeatedBossTypes);
            }
            return data;
        }

        /// <summary>
        /// Guarda en disco los datos de un slot usando <see cref="SaveFile{TData}"/>.
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        /// <param name="data">Datos serializables de la partida.</param>
        /// <returns>Tarea asincrona de escritura.</returns>
        public static Task SaveAsync(int slot, GameData data) => Saves.SaveAsync(slot, data);

        /// <summary>
        /// Carga desde disco los datos de un slot usando <see cref="SaveFile{TData}"/>.
        /// </summary>
        /// <param name="slot">Indice del slot (base 0).</param>
        /// <returns>Datos cargados o <see langword="null"/> si el archivo del slot no existe.</returns>
        public static Task<GameData> LoadAsync(int slot) => Saves.LoadAsync(slot);
    }
}
