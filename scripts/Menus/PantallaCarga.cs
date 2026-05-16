using Godot;
using System;
using System.ComponentModel;

public partial class PantallaCarga : Control
{
    /// <summary>Escena que se cargará después de que la pantalla de carga haya terminado su animación. Se asigna en el editor y debe ser un PackedScene válido para que el cambio de escena funcione correctamente. Es importante asegurarse de que esta propiedad esté configurada correctamente para evitar errores al intentar cambiar a la siguiente escena después de la pantalla de carga.</summary>
	[Export] public PackedScene nextScene;

    /// <summary>
    /// Este método se llama al final de la animación de la pantalla de carga. Se encarga de cambiar a la siguiente escena utilizando el método ChangeSceneToPacked del nodo raíz del árbol de escenas. Es importante que este método se llame al final de la animación para garantizar que la transición a la siguiente escena sea suave y sin interrupciones. Asegúrate de que la animación de la pantalla de carga esté configurada para llamar a este método en el momento adecuado para lograr el efecto deseado.
    /// </summary>
	public void pantallaCargaCompletado()
	{
		GetTree().ChangeSceneToPacked(nextScene);
	}
}
