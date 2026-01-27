using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
    public partial class DashMovementState : State
{

		private PlayerType _player;
        private Timer _dashTimer;


		public override async void Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
            _dashTimer = GetNode<Timer>("DashTimer");

		}

		/// <summary>Al entrar en Running: reproducir animación de carrera y preparar estado.</summary>
		public override void Enter()
		{
			_player.DashAvailable = false;
			_player.SetAnimation("run");
		}
		
		/// <summary>Actualización por frame del estado Running: controla transiciones por physics o input.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void Update(double delta)
		{
		}

		/// <summary>Update de física en Running: aplica la velocidad horizontal en función del input.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void UpdatePhysics(double delta)
		{
			float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
			if (Mathf.Abs(move) > 0f)
				_player.Velocity = new Vector2(move * PlayerType.Speed, _player.Velocity.Y);
			else
				_player.Velocity = new Vector2(0, _player.Velocity.Y);
			_player.MoveAndSlide();
		}

		/// <summary>Procesa input no manejado en Running (ej. pulsación de salto).</summary>
		/// <param name="ev">Evento de entrada recibido.</param>
		public override void HandleInput(InputEvent ev)
		{
			if (ev.IsActionPressed("jump"))
				stateMachine.TransitionTo("JumpingMovementState");
		}
	}
}

