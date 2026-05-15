using Godot;
using System;
using Faeterna.Scripts.Personaje;
using System.Runtime.CompilerServices;

namespace Faeterna.Scripts.Mapa
{
	public partial class Portal : Area2D
	{
		private void OnBodyEnteredPortal(Node2D body)
		{
			if (body is Lira lira)
				GetTree().ChangeSceneToFile("res://scenes/Maps/Poblado.tscn");
		}
	}
}
