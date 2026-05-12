using Chickensoft.SaveFileBuilder;
using Faeterna.Scripts.Personaje;
using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Faeterna.Scripts.Tools
{
    /// <summary>
    /// Registro que contiene los datos de configuración de opciones del juego, incluyendo ajustes de video (antialiasing, vsync, modo de pantalla, FPS, calidad de sombras)
    /// y audio (volumen maestro, música, efectos de sonido, ambiente, sonidos UI). Se utiliza para serializar y deserializar las opciones de usuario.
    /// </summary>
    public record OptionsData(
        int Antialiasing,
        int Vsync,
        int Screen,
        int FpsLock,
        int FpsValue,
        int Shadows,
        float Master,
        float Music,
        float SoundFx,
        float Enviroment,
        float UiSound);

    /// <summary>
    /// Clase centralizada para gestionar el guardado y carga de datos del juego, incluyendo partidas guardadas (slots)
    /// e imágenes de vista previa. Maneja la persistencia de datos de juego utilizando archivos JSON y genera capturas de pantalla como previsualizaciones.
    /// También gestiona opciones de usuario como configuración de audio y video localStorage. Proporciona métodos para guardar, cargar,
    /// eliminar y verificar la existencia de partidas guardadas en diferentes slots.
    /// </summary>
    public class Saves
    {
        /// <summary>Directorio donde se almacenan las imágenes de vista previa de las partidas guardadas.</summary>
        private const string PreviewDirectory = "user://save_previews";

        /// <summary>Ancho (en píxeles) de las imágenes de vista previa generadas de las partidas.</summary>
        private const int PreviewWidth = 512;

        /// <summary>Alto (en píxeles) de las imágenes de vista previa generadas de las partidas.</summary>
        private const int PreviewHeight = 288;

        /// <summary>Opciones de formato JSON para la serialización de datos de partida (indentado para legibilidad).</summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>Archivo de configuración usado para almacenar opciones de usuario (audio, video, etc.).</summary>
        private readonly ConfigFile config = new ConfigFile();

        /// <summary>
        /// Slot activo global para las operaciones de guardado/carga de partida. Este valor determina en qué espacio se guardará
        /// o cargará la partida.
        /// </summary>
        public static int ActiveSlot { get; private set; }

        /// <summary>
        /// Constructor que inicializa el gestor de guardado. Si isSave es verdadero, se prepara para guardar;
        /// si es falso, se carga el archivo de configuración desde el almacenamiento persistente.
        /// </summary>
        /// <param name="isSave">Indica si se inicializa para guardar (verdadero) o para cargar opciones (falso).</param>
        public Saves(bool isSave)
        {
            if (isSave)
            {
                try
                {

                }
                catch (Exception)
                {

                }
            }
            else
            {
                Load("user://settings.cfg");
            }
        }

        /// <summary>
        /// Carga el archivo de configuración desde la ruta especificada. Si el archivo no existe, crea uno nuevo.
        /// </summary>
        /// <param name="filename">Ruta del archivo de configuración a cargar.</param>
        public void Load(string filename)
        {
            try
            {
                config.Load(filename);
            }
            catch (Exception)
            {
                config.Save(filename);
            }
        }

        /// <summary>
        /// Guarda las opciones de usuario (video y audio) en el archivo de configuración persistente.
        /// </summary>
        /// <param name="antialiasing">Opción de antialiasing seleccionada (0-5: Off, FXAA, TAA, MSAA 2x, MSAA 4x, MSAA 8x).</param>
        /// <param name="vsync">Opción de VSync (0: Off, 1: On).</param>
        /// <param name="screen">Modo de pantalla (0: Ventana, 1: Pantalla completa, 2: Sin bordes).</param>
        /// <param name="fpslock">Bloqueo de FPS (0: Off, 1: On).</param>
        /// <param name="fpsValue">Límite de FPS cuando el bloqueo está activado.</param>
        /// <param name="shadows">Nivel de calidad de sombras (0-5: Hard, Soft Very Low, Soft Low, Soft Medium, Soft High, Soft Ultra).</param>
        /// <param name="master">Volumen maestro (0.0-1.0).</param>
        /// <param name="music">Volumen de música (0.0-1.0).</param>
        /// <param name="soundfx">Volumen de efectos de sonido (0.0-1.0).</param>
        /// <param name="enviroment">Volumen de sonidos ambientales (0.0-1.0).</param>
        /// <param name="uisound">Volumen de sonidos de UI (0.0-1.0).</param>
        public void SaveOptions(
            int antialiasing,
            int vsync,
            int screen,
            int fpslock,
            int fpsValue,
            int shadows,
            double master,
            double music,
            double soundfx,
            double enviroment,
            double uisound)
        {
            config.SetValue("Video", "antialiasing", antialiasing);
            config.SetValue("Video", "vsync", vsync);
            config.SetValue("Video", "screen", screen);
            config.SetValue("Video", "fpslock", fpslock);
            config.SetValue("Video", "fps_value", fpsValue);
            config.SetValue("Video", "shadows", shadows);

            config.SetValue("Sound", "master", master);
            config.SetValue("Sound", "music", music);
            config.SetValue("Sound", "soundfx", soundfx);
            config.SetValue("Sound", "enviroment", enviroment);
            config.SetValue("Sound", "uisound", uisound);

            config.Save("user://settings.cfg");
        }

        /// <summary>
        /// Carga las opciones de usuario guardadas y las devuelve como un registro <see cref="OptionsData"/>.
        /// Si alguna opción no está guardada, se usan valores predeterminados.
        /// </summary>
        /// <returns>Registro con las opciones cargadas del archivo de configuración.</returns>
        public OptionsData LoadOptions()
        {
            return new OptionsData(
                (int)config.GetValue("Video", "antialiasing", 0),
                (int)config.GetValue("Video", "vsync", 0),
                (int)config.GetValue("Video", "screen", 0),
                (int)config.GetValue("Video", "fpslock", 0),
                (int)config.GetValue("Video", "fps_value", 60),
                (int)config.GetValue("Video", "shadows", 0),
                (float)(double)config.GetValue("Sound", "master", 1.0),
                (float)(double)config.GetValue("Sound", "music", 1.0),
                (float)(double)config.GetValue("Sound", "soundfx", 1.0),
                (float)(double)config.GetValue("Sound", "enviroment", 1.0),
                (float)(double)config.GetValue("Sound", "uisound", 1.0)
            );
        }

        /// <summary>
        /// Define el slot activo asegurando que no sea negativo. Un valor negativo se reemplaza automáticamente con 0.
        /// </summary>
        /// <param name="slot">Índice del slot a activar (base 0).</param>
        public static void SetActiveSlot(int slot)
        {
            ActiveSlot = Math.Max(0, slot);
        }

        /// <summary>
        /// Verifica si existe un archivo de guardado para el slot especificado.
        /// </summary>
        /// <param name="slot">Índice del slot a verificar (base 0).</param>
        /// <returns>Verdadero si existe el archivo JSON del slot; falso en caso contrario.</returns>
        public static bool HasSave(int slot)
        {
            return FileAccess.FileExists(GetSlotPath(slot));
        }

        /// <summary>
        /// Verifica si el slot activo tiene un archivo de guardado.
        /// </summary>
        /// <returns>Verdadero si existe guardado en el slot activo; falso en caso contrario.</returns>
        public static bool HasSaveInActiveSlot()
        {
            return HasSave(ActiveSlot);
        }

        /// <summary>
        /// Verifica si existe una imagen de vista previa para el slot especificado.
        /// </summary>
        /// <param name="slot">Índice del slot a verificar (base 0).</param>
        /// <returns>Verdadero si existe la imagen PNG del slot; falso en caso contrario.</returns>
        public static bool HasPreview(int slot)
        {
            return FileAccess.FileExists(GetSlotPreviewPath(slot));
        }

        /// <summary>
        /// Carga la textura de vista previa del slot desde el almacenamiento persistente.
        /// </summary>
        /// <param name="slot">Índice del slot de cuya previsualizacion se cargará (base 0).</param>
        /// <returns>Una <see cref="Texture2D"/> lista para usar en UI, o nulo si no existe o está corrupta.</returns>
        public static Texture2D LoadPreviewTexture(int slot)
        {
            if (!HasPreview(slot))
            {
                return null;
            }

            string absolutePath = ProjectSettings.GlobalizePath(GetSlotPreviewPath(slot));
            Image image = Image.LoadFromFile(absolutePath);
            if (image == null || image.IsEmpty())
            {
                return null;
            }

            return ImageTexture.CreateFromImage(image);
        }

        /// <summary>
        /// Elimina completamente un slot incluyendo su archivo de guardado JSON y su imagen de vista previa PNG.
        /// </summary>
        /// <param name="slot">Índice del slot a eliminar (base 0).</param>
        public static void DeleteSlot(int slot)
        {
            DeleteFileIfExists(GetSlotPath(slot), $"save slot {slot}");
            DeleteFileIfExists(GetSlotPreviewPath(slot), $"preview slot {slot}");
        }

        /// <summary>
        /// Guarda el progreso de un checkpoint y genera una imagen de vista previa del estado actual del juego.
        /// </summary>
        /// <param name="player">Instancia del jugador desde la que se extrae el estado para guardar.</param>
        /// <param name="checkpointId">Identificador único del checkpoint alcanzado.</param>
        /// <param name="scenePath">Ruta de la escena actual para validar cargas posteriores.</param>
        /// <param name="checkpointPosition">Posición de respawn a persistir cuando se cargue desde este checkpoint.</param>
        /// <param name="viewport">Viewport fuente para la captura de pantalla; si es nulo, usa el del jugador.</param>
        /// <returns>Tarea asincrónica que completa cuando se guardan los datos y la previsualizacion.</returns>
        public static async Task SaveCheckpointAsync(Lira player, string checkpointId, string scenePath, Vector2 checkpointPosition, Viewport viewport = null)
        {
            string previewPath = GetSlotPreviewPath(ActiveSlot);
            SavePreview(viewport ?? player.GetViewport(), previewPath);

            var data = new GameData
            {
                SaveSlot = ActiveSlot,
                ScenePath = scenePath,
                LastCheckpointId = checkpointId,
                PreviewImagePath = previewPath,
                PlayerData = player.BuildSaveData(checkpointPosition)
            };

            await SaveAsync(ActiveSlot, data);
        }

        /// <summary>
        /// Carga los datos de la partida guardada en el slot activo.
        /// </summary>
        /// <returns>Datos cargados desde el slot activo, o nulo si el archivo no existe.</returns>
        public static Task<GameData> LoadActiveSlotAsync()
        {
            return LoadAsync(ActiveSlot);
        }

        /// <summary>
        /// Guarda los datos de una partida en el slot especificado usando el sistema de archivo persistente.
        /// Los datos se serializan como JSON indentado para legibilidad.
        /// </summary>
        /// <param name="slot">Índice del slot en el que se guardarán los datos (base 0).</param>
        /// <param name="data">Datos serializables de la partida a guardar.</param>
        /// <returns>Tarea asincrónica que completa cuando se escriben los datos en disco.</returns>
        public static async Task SaveAsync(int slot, GameData data)
        {
            string path = GetSlotPath(slot);

            var root = new SaveChunk<GameData>(
                onSave: (_) => data,
                onLoad: (_, _) => { }
            );

            var saveFile = new SaveFile<GameData>(
                root,
                async saveData =>
                {
                    string json = JsonSerializer.Serialize(saveData, JsonOptions);
                    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
                    file.StoreString(json);
                    await Task.CompletedTask;
                },
                async () =>
                {
                    if (!FileAccess.FileExists(path))
                    {
                        return new GameData();
                    }

                    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                    string json = file.GetAsText();
                    GameData loaded = JsonSerializer.Deserialize<GameData>(json, JsonOptions);
                    await Task.CompletedTask;
                    return loaded ?? new GameData();
                }
            );

            await saveFile.Save();
        }

        /// <summary>
        /// Carga los datos de una partida desde el slot especificado desde el almacenamiento persistente.
        /// </summary>
        /// <param name="slot">Índice del slot del que se cargarán los datos (base 0).</param>
        /// <returns>Datos de la partida cargados, o nulo si el archivo del slot no existe.</returns>
        public static async Task<GameData> LoadAsync(int slot)
        {
            string path = GetSlotPath(slot);
            if (!FileAccess.FileExists(path))
            {
                return null;
            }

            GameData loadedData = null;

            var root = new SaveChunk<GameData>(
                onSave: (_) => new GameData(),
                onLoad: (_, data) => loadedData = data
            );

            var saveFile = new SaveFile<GameData>(
                root,
                async saveData =>
                {
                    string json = JsonSerializer.Serialize(saveData, JsonOptions);
                    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
                    file.StoreString(json);
                    await Task.CompletedTask;
                },
                async () =>
                {
                    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                    string json = file.GetAsText();
                    GameData loaded = JsonSerializer.Deserialize<GameData>(json, JsonOptions);
                    await Task.CompletedTask;
                    return loaded ?? new GameData();
                }
            );

            await saveFile.Load();
            return loadedData;
        }

        /// <summary>
        /// Genera la ruta del archivo JSON para un slot específico.
        /// </summary>
        /// <param name="slot">Índice del slot (base 0).</param>
        /// <returns>Ruta del archivo JSON del slot.</returns>
        private static string GetSlotPath(int slot)
        {
            return $"user://save_slot_{slot + 1}.json";
        }

        /// <summary>
        /// Genera la ruta del archivo PNG de vista previa para un slot específico.
        /// </summary>
        /// <param name="slot">Índice del slot (base 0).</param>
        /// <returns>Ruta del archivo PNG de previsualizacion del slot.</returns>
        private static string GetSlotPreviewPath(int slot)
        {
            return $"{PreviewDirectory}/slot_{slot + 1}.png";
        }

        /// <summary>
        /// Captura una imagen del viewport especificado, la redimensiona al tamaño estándar de previsualizacion
        /// y la guarda como archivo PNG en la ruta especificada.
        /// </summary>
        /// <param name="viewport">Viewport de cuya textura se capturará la image.</param>
        /// <param name="previewPath">Ruta donde se guardará la imagen PNG.</param>
        private static void SavePreview(Viewport viewport, string previewPath)
        {
            if (viewport == null)
            {
                return;
            }

            EnsurePreviewDirectory();

            Image image = viewport.GetTexture()?.GetImage();
            if (image == null || image.IsEmpty())
            {
                return;
            }

            image.Resize(PreviewWidth, PreviewHeight, Image.Interpolation.Lanczos);

            string absolutePath = ProjectSettings.GlobalizePath(previewPath);
            Error saveResult = image.SavePng(absolutePath);
            if (saveResult != Error.Ok)
            {
                GD.PushWarning($"No se pudo guardar el preview del slot {ActiveSlot}: {saveResult}");
            }
        }

        /// <summary>
        /// Crea el directorio de previsualizaciones si no existe. Necesario antes de guardar imágenes de vista previa.
        /// </summary>
        private static void EnsurePreviewDirectory()
        {
            string absolutePath = ProjectSettings.GlobalizePath(PreviewDirectory);
            DirAccess.MakeDirRecursiveAbsolute(absolutePath);
        }

        /// <summary>
        /// Intenta eliminar un archivo del almacenamiento persistente. Imprime un aviso si la eliminación falla.
        /// </summary>
        /// <param name="userPath">Ruta del archivo a eliminar.</param>
        /// <param name="fileDescription">Descripción del archivo para mensajes de log (ej: "save slot 1").</param>
        private static void DeleteFileIfExists(string userPath, string fileDescription)
        {
            if (!FileAccess.FileExists(userPath))
            {
                return;
            }

            string absolutePath = ProjectSettings.GlobalizePath(userPath);
            Error result = DirAccess.RemoveAbsolute(absolutePath);
            if (result != Error.Ok)
            {
                GD.PushWarning($"No se pudo borrar {fileDescription}: {result}");
            }
        }
    }
}
