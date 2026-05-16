using Godot;
using Faeterna.scripts.Enemigos.ReyJabali;
using Faeterna.Scripts.Personaje;

namespace Faeterna.scripts.Mapa
{
	/// <summary>
	/// Área de combate del boss. Controla la aparición del objeto de dash,
	/// la música de fondo del combate y reacciona a la muerte de ReyJabali.
	/// </summary>
	public partial class BossArea : Area2D
    {
		/// <summary>
		/// Nodo del objeto dash que se desbloquea cuando el boss muere.
		/// </summary>
		[Export] private Node2D _dashItem;

		/// <summary>
		/// Referencia al boss ReyJabali para escuchar su señal de muerte.
		/// </summary>
		[Export] private ReyJabali _reyJabali;

		/// <summary>
		/// Reproductor de audio que se usa para iniciar la música del combate.
		/// </summary>
		[Export] private AudioStreamPlayer _backgroundAudioPLayer;

		/// <summary>
		/// Audio de fondo que se reproduce al entrar el jugador al área.
		/// </summary>
		[Export] private AudioStream _backgroundAudio;

		/// <summary>
		/// Inicializa el área del boss, oculta el objeto de dash y conecta la señal de muerte del boss.
		/// </summary>
		public override void _Ready()
		{
			_dashItem.Visible = false;

			// Conectar la señal de muerte del boss
			if (_reyJabali != null)
			{
				_reyJabali.jabaliBossdeath += OnJabaliBossDeath;
			}
			else
			{
				GD.PrintErr("ReyJabali no está asignado en BossArea");
			}
		}

		/// <summary>
		/// Se llama cuando un cuerpo entra en el área. Si es Lira, reproduce la música del combate.
		/// </summary>
		/// <param name="body">Cuerpo que entró en el área.</param>
		public void _on_body_entered(Node2D body)
		{
			if (_backgroundAudioPLayer == null || _backgroundAudio == null || body is not Lira)
				return;

			_backgroundAudioPLayer.Stream = _backgroundAudio;
			_backgroundAudioPLayer.Play();


		}

		/// <summary>
		/// Se ejecuta cuando ReyJabali muere. Muestra el objeto de dash.
		/// </summary>
		public void OnJabaliBossDeath()
		{
			GD.Print("Jabali Boss ha muerto. Activando dash item...");
			_dashItem.Visible = true;
		}

	}
}
