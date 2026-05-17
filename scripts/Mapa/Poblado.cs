using Faeterna.Scripts.Tools;
using Godot;
using System;

public partial class Poblado : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	 	GameSaveService.RequestLoadActiveSlotOnNextScene();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
