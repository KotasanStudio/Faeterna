using Godot;
using System;

public partial class LogoAnimationScene : Control
{
    /// <summary>Control que representa la escena de animación del logo al iniciar el juego. Esta escena se muestra al cargar el juego y reproduce una animación del logo durante un tiempo determinado. Después de que la animación termine, se cambia automáticamente a la escena del menú principal. Además, si el jugador presiona la tecla de pausa durante la animación, se detiene el temporizador y se cambia inmediatamente a la escena del menú principal. Es importante que el temporizador esté configurado correctamente para que la transición entre escenas funcione sin problemas y que el jugador tenga la opción de omitir la animación si lo desea.</summary>
	[Export] private Timer logoTimer;

    /// <summary>
    /// En el método _Input se verifica si el jugador ha presionado la tecla de pausa. Si es así, se detiene el temporizador de la animación del logo y se cambia inmediatamente a la escena del menú principal. Esto permite que los jugadores puedan omitir la animación si lo desean, proporcionando una experiencia de usuario más flexible. Es importante que esta función esté implementada correctamente para garantizar que el juego responda a la entrada del jugador durante la animación del logo y permita una transición suave al menú principal cuando se presione la tecla de pausa.
    /// </summary>
    /// <param name="event">
    /// El evento de entrada que se está procesando. Este parámetro contiene información sobre la acción que se ha realizado, como qué tecla se ha presionado. En este caso, se verifica si la acción "pause" ha sido activada para determinar si se debe omitir la animación del logo y cambiar al menú principal. Es importante que el evento de entrada esté configurado correctamente en el proyecto para que esta función funcione como se espera.
    /// </param>
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
			logoTimer.Stop();
             GetTree().ChangeSceneToFile("res://scenes/Menus/MainMenu.tscn");
        }
    }

    /// <summary>
    /// Este método se llama cuando el temporizador de la animación del logo alcanza su tiempo límite. Se encarga de cambiar a la escena del menú principal utilizando el método ChangeSceneToFile del nodo raíz del árbol de escenas. Es importante que este método esté configurado para ser llamado al final de la animación del logo para garantizar una transición suave al menú principal una vez que la animación haya terminado. Asegúrate de que el temporizador esté configurado correctamente para llamar a este método en el momento adecuado durante la animación.
    /// </summary>
	public void _on_logo_timer_timeout()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menus/MainMenu.tscn");
    }
}
