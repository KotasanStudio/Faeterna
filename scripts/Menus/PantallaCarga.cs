using Godot;
using System;
using System.ComponentModel;

public partial class PantallaCarga : Control
{
	[Export] public PackedScene nextScene;

	public void pantallaCargaCompletado()
	{
		GetTree().ChangeSceneToPacked(nextScene);
	}
}
