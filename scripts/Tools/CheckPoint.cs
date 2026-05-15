using Godot;
using Faeterna.Scripts.Personaje;
using PlayerType = Faeterna.Scripts.Personaje.Lira;
using System.Threading.Tasks; // Necesario para Task

namespace Faeterna.Scripts.Tools
{
    public partial class CheckPoint : Node2D
    {
        [Export] private string _checkpointId = "checkpoint_01";
        [Export] private Node2D _spawnPoint;

        public async Task ActionSaveProgress(Lira player)
        {
            if (GetTree().Paused) return;
            GD.Print($"Checkpoint {Name} activado. Guardando progreso...");
            string scenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
            Vector2 savePosition = _spawnPoint?.GlobalPosition ?? GlobalPosition;

            await GameSaveService.SaveCheckpointAsync(player, _checkpointId, scenePath, savePosition);
            GD.Print($"Checkpoint guardado ({_checkpointId}) mediante interacción.");
        }

        public Vector2 GetPrayPosition() => GlobalPosition;
        
        public void OnAreaCheckPointEntered(Node2D body) 
        {
        }
    }
}