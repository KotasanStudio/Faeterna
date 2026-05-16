using Godot;
using System;
using Faeterna.scripts.Enemigos.ReyJabali;
using Faeterna.Scripts.Personaje;

public partial class BossArea : Area2D
{

	[Export] private Node2D _dashItem;
	[Export] private ReyJabali _reyJabali;
	[Export] private AudioStreamPlayer _backgroundAudioPLayer;
	[Export] private AudioStream _backgroundAudio;

	
	public override void _Ready()
    {
        _dashItem.Visible = false;

		// Conectar la señal de muerte del boss
		if (_reyJabali != null)
		{
			_reyJabali.jabaliBossdeath += onJabaliBossDeath;
		}
		else
		{
			GD.PrintErr("ReyJabali no está asignado en BossArea");
		}
    }

	public void _on_body_entered(Node2D body)
	{
		if (_backgroundAudioPLayer == null || _backgroundAudio == null || body is not Lira lira)
			return;

			_backgroundAudioPLayer.Stream = _backgroundAudio;
			_backgroundAudioPLayer.Play();


	}

	public void onJabaliBossDeath()
	{
		GD.Print("Jabali Boss ha muerto. Activando dash item...");
		_dashItem.Visible = true;
	}

}
