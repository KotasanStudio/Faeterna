using Faeterna.scripts.Player;
using Godot;
using System;

namespace Faeterna.Scripts.Slime
{
    public partial class Slime2D : CharacterBody2D
    {
        private AnimatedSprite2D _animatedSprite;
        [Export] public float jump_speed { get; set; }
        [Export] public float speed { get; set; }

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public void set_animation(string animation_name)
        {
            if (_animatedSprite != null)
            {
                _animatedSprite.Play(animation_name);
            }
        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota.GetType() == typeof(Lira))
            {
                GD.Print("Casi Auuuuuuuuuuuuuu");
                if (MoveAndSlide())
                {
                    for (int i = 0; i < GetSlideCollisionCount(); ++i)
                    {
                        KinematicCollision2D collision = GetSlideCollision(i);
                        GD.Print("Auuuuuuuuuuuuuu");
                        if (collision.GetCollider().GetType() == typeof(Lira))
                            ((Lira)collision.GetCollider()).TakeDamage(1);
                    }
                }
            }
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota.GetType() == typeof(Lira))
            {
                // Lógica de detección pendiente de implementar.
            }
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota.GetType() == typeof(Lira))
            {
                // Lógica de salida de detección pendiente de implementar.
            }
        }
    }
}
