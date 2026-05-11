using Faeterna.scripts.Personaje;
using Godot;
using System.Threading.Tasks;

namespace Faeterna.scripts.Tools
{
    /// <summary>
    /// Fachada de compatibilidad. La lógica real de guardado está centralizada en <see cref="Saves"/>.
    /// </summary>
    public static class GameSaveService
    {
        public static int ActiveSlot => Saves.ActiveSlot;

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
        public static Task SaveCheckpointAsync(Lira player, string checkpointId, string scenePath, Vector2 checkpointPosition, Viewport viewport = null)
            => Saves.SaveCheckpointAsync(player, checkpointId, scenePath, checkpointPosition, viewport);

        /// <summary>
        /// Carga los datos del slot activo.
        /// </summary>
        /// <returns>Datos cargados o <see langword="null"/> si el archivo no existe.</returns>
        public static Task<GameData> LoadActiveSlotAsync() => Saves.LoadActiveSlotAsync();

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
