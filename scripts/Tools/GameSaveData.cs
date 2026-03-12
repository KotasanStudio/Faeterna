using Godot;
using System.Text.Json.Serialization;

namespace Faeterna.scripts.Tools
{
    public class GameData
    {
        public PlayerSaveData PlayerData { get; set; } = new();
    }

    public class PlayerSaveData
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public int Health { get; set; } = 5;
        public float Mana { get; set; } = 100f;

        [JsonIgnore]
        public Vector3 Position
        {
            get => new(PositionX, PositionY, PositionZ);
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
                PositionZ = value.Z;
            }
        }
    }
}
