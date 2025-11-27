using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
	public partial class RunningMovementState : State
	{
		private PlayerType _player;

		public override async void Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("NinjaFrogGroup");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}

		/// <summary>Al entrar en Running: reproducir animación de carrera y preparar estado.</summary>
		public override void Enter()
		{
			_player.DoubleJumpAvailable = true;
			_player.SetAnimation("run");
		}
		
		/// <summary>Actualización por frame del estado Running: controla transiciones por physics o input.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void Update(double delta)
		{
			if (!_player.IsOnFloor())
			{
				GD.Print("Transitioning to falling or jumping state from running.");
				if (_player.Velocity.Y < 0)
					stateMachine.TransitionTo("JumpingMovementState");
				else 
					stateMachine.TransitionTo("FallingMovementState");

				return;
			}
			if (Mathf.Abs((float)_player.Velocity.X) < 0.1f)
			{
				bool inputPressed = Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right");
				if (!inputPressed)
				{
					GD.Print("Transitioning to idle state from running.");
					stateMachine.TransitionTo("IdleMovementState");
				}
			}
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
			if (ev.IsActionPressed("jump") && _player.IsOnFloor())
				stateMachine.TransitionTo("JumpingMovementState");
		}
	}
}
