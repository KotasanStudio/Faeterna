using Godot;
using Faeterna.Scripts.Personaje;
using PlayerType = Faeterna.Scripts.Personaje.Lira;


namespace Faeterna.Scripts.Tools
{
    public partial class CheckPoint : Node2D
    {
        private Lira _player;
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

            _player = player;

            string scenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
            Vector2 savePosition = _spawnPoint?.GlobalPosition ?? GlobalPosition;

            await GameSaveService.SaveCheckpointAsync(_player, _checkpointId, scenePath, savePosition);
            GD.Print($"Checkpoint guardado ({_checkpointId}) en slot {GameSaveService.ActiveSlot + 1}.");
        }
    }
}

