using Godot;
using System.Text.Json.Serialization;

namespace Faeterna.scripts.Tools
{
    public class GameData
    {
        public int SaveSlot { get; set; }
        public string ScenePath { get; set; } = string.Empty;
        public string LastCheckpointId { get; set; } = string.Empty;
        public string PreviewImagePath { get; set; } = string.Empty;
        public PlayerSaveData PlayerData { get; set; } = new();
    }

    public class PlayerSaveData
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public int Health { get; set; } = 5;
        public float Mana { get; set; } = 100f;
        public bool DoubleJumpAvailable { get; set; } = true;
        public bool DashAvailable { get; set; } = true;
        public bool CoyoteAvailable { get; set; } = true;

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
