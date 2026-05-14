using Godot;
using Faeterna.Scripts.Personaje;

namespace Faeterna.Scripts.Tutorial
{
    public partial class Turotial : Node2D
    {
        [Export] private Node2D _control;

        public void _on_area_2d_body_entered(Node body)
        {
            GD.Print("Tutorial detectó colisión con: " + body.Name);
            if (body is CharacterBody2D)
            {
                GD.Print("¡Lira entró en el área!");
                DesvanecerNodo();
            }
        }

        public void DesvanecerNodo()
        {
            if (_control == null)
            {
                GD.PrintErr("ERROR: _control es null!");
                return;
            }

            GD.Print("Iniciando desvanecimiento del nodo Controles");

            // Asegura que esté visible al arrancar la animación
            _control.Visible = true;

            // Crea el Tween para gestionar la transición fluida
            Tween tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Linear);
            tween.SetEase(Tween.EaseType.InOut);

            // Conserva el color actual de modulación (R, G, B) pero fija el Alfa (A) en 0.0f
            Color colorDestino = new Color(
                _control.Modulate.R,
                _control.Modulate.G,
                _control.Modulate.B,
                0.0f
            );

            GD.Print($"Animando desde {_control.Modulate} a {colorDestino}");

            // Anima la propiedad 'modulate' del Node2D hacia el color transparente en 1.5 segundos
            tween.TweenProperty(
                _control,
                "modulate",
                colorDestino,
                1.5f
            );

            // Al finalizar, oculta el nodo por completo para optimizar el rendimiento
            tween.Finished += () =>
            {
                GD.Print("Tween finalizado");
                _control.Visible = false;
            };
        }
    }
}
