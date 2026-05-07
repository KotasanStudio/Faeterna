using Faeterna.scripts.Player;
using Godot;
using System;

namespace Faeterna.Scripts.Slime
{
    /// <summary>
    /// Enemigo "Slime" que representa una criatura básica de 2D que puede saltar y atacar al jugador.
    /// El slime tiene un sistema de detección que responde cuando el jugador se acerca, y un área de ataque que inflige daño al jugador si entra en contacto.
    /// Los estados se controlan mediante animaciones y el slime puede ser destruido por el jugador.
    /// </summary>
    public partial class Slime2D : CharacterBody2D
    {
        /// <summary>Referencia al nodo AnimatedSprite2D hijo que muestra las animaciones del slime.</summary>
        private AnimatedSprite2D _animatedSprite;

        /// <summary>Velocidad de salto del slime (qué tan rápido salta hacia arriba).</summary>
        [Export] public float jump_speed { get; set; }

        /// <summary>Velocidad de movimiento horizontal del slime (qué tan rápido se mueve).</summary>
        [Export] public float speed { get; set; }

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Obtiene la referencia al nodo AnimatedSprite2D hijo para usar sus animaciones.
        /// </summary>
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Reproduce una animación en el nodo AnimatedSprite2D del slime. Método auxiliar usado para cambiar la animación.
        /// </summary>
        /// <param name="animation_name">Nombre de la animación a reproducir.</param>
        public void set_animation(string animation_name)
        {
            if (_animatedSprite != null)
            {
                _animatedSprite.Play(animation_name);
            }
        }

        /// <summary>
        /// Se llama cuando algo entra en el área de ataque del slime. Verifica si es el jugador (Lira) y le inflige daño
        /// si entra en contacto con el slime detectando colisiones de física.
        /// </summary>
        /// <param name="prota">Nodo 2D que entró en el área de ataque.</param>
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

        /// <summary>
        /// Se llama cuando algo entra en el área de detección del slime. Verifica si es el jugador (Lira).
        /// La lógica de respuesta a la detección está pendiente de implementar.
        /// </summary>
        /// <param name="prota">Nodo 2D que entró en el área de detección.</param>
        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota.GetType() == typeof(Lira))
            {
                // Lógica de detección pendiente de implementar.
            }
        }

        /// <summary>
        /// Se llama cuando algo sale del área de detección del slime. Verifica si es el jugador (Lira).
        /// La lógica de salida de detección está pendiente de implementar.
        /// </summary>
        /// <param name="prota">Nodo 2D que salió del área de detección.</param>
        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota.GetType() == typeof(Lira))
            {
                // Lógica de salida de detección pendiente de implementar.
            }
        }
    }
}
