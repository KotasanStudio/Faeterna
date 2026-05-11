using Chickensoft.SaveFileBuilder;
using Faeterna.Scripts.Personaje;
using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Faeterna.scripts.Tools
{
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

    public class Saves
    {
        private const string PreviewDirectory = "user://save_previews";
        private const int PreviewWidth = 512;
        private const int PreviewHeight = 288;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly ConfigFile config = new ConfigFile();

        /// <summary>
        /// Slot activo global para las operaciones de guardado/carga de partida.
        /// </summary>
        public static int ActiveSlot { get; private set; }

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
        /// Define el slot activo asegurando que no sea negativo.
        /// </summary>
        public static void SetActiveSlot(int slot)
        {
            ActiveSlot = Math.Max(0, slot);
        }

        /// <summary>
        /// Indica si existe archivo de guardado para un slot.
        /// </summary>
        public static bool HasSave(int slot)
        {
            return FileAccess.FileExists(GetSlotPath(slot));
        }

        /// <summary>
        /// Indica si el slot activo tiene guardado.
        /// </summary>
        public static bool HasSaveInActiveSlot()
        {
            return HasSave(ActiveSlot);
        }

        /// <summary>
        /// Indica si existe preview para un slot.
        /// </summary>
        public static bool HasPreview(int slot)
        {
            return FileAccess.FileExists(GetSlotPreviewPath(slot));
        }

        /// <summary>
        /// Carga la textura preview del slot.
        /// </summary>
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
        /// Elimina guardado y preview del slot.
        /// </summary>
        public static void DeleteSlot(int slot)
        {
            DeleteFileIfExists(GetSlotPath(slot), $"save slot {slot}");
            DeleteFileIfExists(GetSlotPreviewPath(slot), $"preview slot {slot}");
        }

        /// <summary>
        /// Guarda progreso de checkpoint y genera preview del slot activo.
        /// </summary>
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
        /// Carga el slot activo.
        /// </summary>
        public static Task<GameData> LoadActiveSlotAsync()
        {
            return LoadAsync(ActiveSlot);
        }

        /// <summary>
        /// Guarda datos de juego en el slot indicado.
        /// </summary>
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
        /// Carga datos de juego desde el slot indicado.
        /// </summary>
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

        private static string GetSlotPath(int slot)
        {
            return $"user://save_slot_{slot + 1}.json";
        }

        private static string GetSlotPreviewPath(int slot)
        {
            return $"{PreviewDirectory}/slot_{slot + 1}.png";
        }

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

        private static void EnsurePreviewDirectory()
        {
            string absolutePath = ProjectSettings.GlobalizePath(PreviewDirectory);
            DirAccess.MakeDirRecursiveAbsolute(absolutePath);
        }

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
