using Godot;
using System;

public partial class Bosque : Node2D
{

    [Export] private AudioStreamPlayer _backgroundMusic;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        if (_backgroundMusic != null)
		{
			_backgroundMusic.Play();
		}
		else
		{
			GD.PrintErr("AudioStreamPlayer2D para música de fondo no asignado en Bosque");
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
