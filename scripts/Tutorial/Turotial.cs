using Godot;
using Faeterna.Scripts.Personaje;

namespace Faeterna.Scripts.Tutorial
{
    public partial class Turotial : Node2D
    {
        /// <summary>Nodo de control que se desvanece o desaparece al entrar Lira en el área. Se espera que este nodo sea un hijo directo de este nodo y se use para mostrar información o guías durante el tutorial. Al detectar la colisión con Lira, se llama al método DesvanecerNodo para eliminar o ocultar este nodo de control, permitiendo que el jugador avance en el tutorial sin distracciones. Es importante asegurarse de que este nodo esté correctamente asignado en el editor para evitar errores al intentar acceder a él desde el código.</summary>
        [Export] private Node2D _control;

        /// <summary>
        /// Método que se llama cuando un cuerpo entra en el área 2D del tutorial. Si el cuerpo es una instancia de CharacterBody2D (lo que indica que es Lira), se imprime un mensaje en la consola y se llama al método DesvanecerNodo para eliminar o ocultar el nodo de control asociado al tutorial. Este método es esencial para detectar la interacción del jugador con el área del tutorial y permitir que el juego responda adecuadamente a esa interacción, proporcionando una experiencia de aprendizaje fluida para el jugador. Es importante asegurarse de que este método esté conectado correctamente a la señal de entrada de cuerpo del área 2D para que funcione como se espera.
        /// </summary>
        /// <param name="body">
        /// El cuerpo que ha entrado en el área 2D del tutorial. Este parámetro se espera que sea una instancia de CharacterBody2D, lo que indica que es Lira, el personaje principal del juego. Si el cuerpo no es una instancia de CharacterBody2D, no se realizará ninguna acción adicional. Es importante asegurarse de que el área 2D esté configurada correctamente para detectar la entrada de cuerpos y que este método esté conectado a la señal correspondiente para garantizar su funcionamiento adecuado.
        /// </param>
        public void _on_area_2d_body_entered(Node body)
        {
            GD.Print("Tutorial detectó colisión con: " + body.Name);
            if (body is CharacterBody2D)
            {
                GD.Print("¡Lira entró en el área!");
                DesvanecerNodo();
            }
        }

        /// <summary>
        /// Método que se encarga de desvanecer o eliminar el nodo de control asociado al tutorial. Si el nodo de control es null, se imprime un mensaje de error en la consola. De lo contrario, se llama al método QueueFree para eliminar el nodo de control del árbol de escenas, lo que hace que desaparezca visualmente del juego. Este método es esencial para proporcionar una experiencia de tutorial fluida, permitiendo que el jugador avance sin distracciones una vez que haya interactuado con el área del tutorial. Es importante asegurarse de que este método esté implementado correctamente para evitar errores al intentar acceder o eliminar el nodo de control.
        /// </summary>
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
