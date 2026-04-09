using Godot;
using Faeterna.scripts.Player;

namespace Faeterna.scripts.Tools
{
    public partial class CheckPoint : Node2D
    {
        [Export] private string _checkpointId = "checkpoint_01";
        [Export] private Node2D _spawnPoint;

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

