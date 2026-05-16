using Godot;
using System;
using Faeterna.Scripts.Personaje;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Faeterna.Scripts.Mapa
{
	public partial class Portal : Area2D
	{

		public void onBodyEnteredPortal(Node2D body)
		{
			if (body is Lira lira)
				CallDeferred(MethodName.ChangeScene);


		}

		private void ChangeScene()
		{
			GetTree().ChangeSceneToFile("res://scenes/Menus/pantalla_carga.tscn");
		}
	}
}
