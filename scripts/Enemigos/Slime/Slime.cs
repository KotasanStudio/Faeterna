using Faeterna.scripts.Player;
using Godot;
using System;

namespace Faeterna.scripts.Slime
{
    public partial class Slime : CharacterBody2D
    {
        private AnimatedSprite2D _animatedSprite;

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
            GD.Print("Slime hit box entered by: " + prota.Name);
            if (prota is Lira lira)
            {
                lira.TakeDamage(1, GlobalPosition);
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