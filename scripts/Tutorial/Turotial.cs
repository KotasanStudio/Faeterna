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
            QueueFree();
        }
    }
}
