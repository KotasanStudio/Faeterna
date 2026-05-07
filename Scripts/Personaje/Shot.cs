using Godot;
using System;

namespace Faeterna.Scripts.Personaje
{
    public partial class Shot : Area2D
    {
        [Export] private float Speed = 450f;

        [Export] private AnimatedSprite2D _animatedSprite;
        public Vector2 Direction = Vector2.Zero;
        public float ManaCost = 20f;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            

            if (Direction == Vector2.Zero)
                return;

            Translate(Direction * Speed * (float)delta);
            if (Direction.X < 0f)
                _animatedSprite.FlipH = true;
            else if (Direction.X > 0f)
                _animatedSprite.FlipH = false;

        }

        /// <summary>
        /// Handles logic when another Area2D enters the monitored area.
        /// </summary>
        /// <param name="area">The Area2D instance that has entered the area. Cannot be null.</param>
        private void OnAreaEntered(Area2D area)
        {
            HandleCollision(area);
        }

        /// <summary>Elimina la bala al chocar con el terreno.</summary>
        private void OnBodyEntered(Node body)
        {
            HandleCollision(body);
        }

        private void HandleCollision(Node node)
        {
            if (node.IsInGroup("Terreno"))
            {
                QueueFree();
                return;
            }
        }
    }
}
