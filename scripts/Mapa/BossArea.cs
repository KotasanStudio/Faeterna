using Godot;
using System;
using Faeterna.scripts.Enemigos.ReyJabali;

public partial class BossArea : Area2D
{

	[Export] private Node2D _dashItem;
	[Export] private ReyJabali _reyJabali;
	
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

	public void onJabaliBossDeath()
	{
		GD.Print("Jabali Boss ha muerto. Activando dash item...");
		_dashItem.Visible = true;
	}

}
