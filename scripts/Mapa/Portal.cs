using Godot;
using System;
using Faeterna.Scripts.Personaje;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Faeterna.Scripts.Mapa
{
	public partial class Portal : Area2D
	{

        /// <summary>
        /// Método que se llama cuando un cuerpo entra en el área del portal. Si el cuerpo es una instancia de Lira, se llama al método ChangeScene de forma diferida para cambiar a la escena de carga. Es importante usar CallDeferred para asegurarse de que el cambio de escena se realice después de que se hayan procesado todas las señales y eventos actuales, evitando posibles conflictos o errores durante la transición entre escenas.
        /// </summary>
        /// <param name="body">
        /// El cuerpo que ha entrado en el área del portal. Este parámetro se espera que sea una instancia de Lira, que es el personaje principal del juego. Si el cuerpo no es una instancia de Lira, no se realizará ningún cambio de escena. Es importante asegurarse de que el cuerpo que entra en el portal sea el personaje correcto para evitar cambios de escena no deseados.
        /// </param>
		public void onBodyEnteredPortal(Node2D body)
		{
			if (body is Lira lira)
				CallDeferred(MethodName.ChangeScene);
		}

        /// <summary>
        /// Método que se encarga de cambiar a la escena de carga. Se llama de forma diferida desde el método onBodyEnteredPortal para garantizar que el cambio de escena se realice después de que se hayan procesado todas las señales y eventos actuales. Este método utiliza el método ChangeSceneToFile del nodo raíz del árbol de escenas para cambiar a la escena de carga ubicada en "res://scenes/Menus/pantalla_carga.tscn". Es importante asegurarse de que la ruta a la escena de carga sea correcta para evitar errores al intentar cambiar a esa escena.
        /// </summary>
		private void ChangeScene()
		{
			GetTree().ChangeSceneToFile("res://scenes/Menus/pantalla_carga.tscn");
		}
	}
}
