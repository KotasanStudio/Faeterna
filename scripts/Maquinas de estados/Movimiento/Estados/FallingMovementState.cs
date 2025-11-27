using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
	public partial class FallingMovementState : State
	{
		private PlayerType _player;

		public override async void Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}
		
		/// <summary>Al entrar en Falling: reproducir animación de caída.</summary>
		public override void Enter()
		{
			_player.SetAnimation("fall");
		}

		/// <summary>Actualización por frame en Falling: transiciones al aterrizar o doble salto.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void Update(double delta)
		{
			if (_player.IsOnFloor() && _player.Velocity.X == 0)
			{
				GD.Print("Transitioning to idle state from falling.");
				stateMachine.TransitionTo("IdleMovementState");
			}
		}

		/// <summary>Update de física en Falling: aplica gravedad y control horizontal inmediato en aire.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void UpdatePhysics(double delta)
		{
			if (!_player.IsOnFloor())
			{
				Vector2 velocity = _player.Velocity;
				velocity += _player.GetGravity() * (float)delta;
				float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
				if (Mathf.Abs(move) > 0f)
					velocity.X = move * PlayerType.Speed;
				else
					velocity.X = 0f;
				_player.Velocity = velocity;
				_player.MoveAndSlide();
			}

			if (_player.IsOnFloor())
			{
				if (Mathf.Abs(_player.Velocity.X) > 0.1f)
				{
					GD.Print("Transitioning to running state from jumping (landed).");
					stateMachine.TransitionTo("RunningMovementState");
				}
				else
				{
					GD.Print("Transitioning to idle state from jumping (landed).");
					stateMachine.TransitionTo("IdleMovementState");
				}
			}
		}

		/// <summary>Procesa eventos de entrada mientras se está en Falling.</summary>
		/// <param name="ev">Evento de entrada recibido.</param>
		public override void HandleInput(InputEvent ev)
		{
			if (ev.IsActionPressed("jump") && _player.CoyoteTimeCounter > 0f)
			{
				stateMachine.TransitionTo("JumpingMovementState");
				return;
			}
			if (ev.IsActionPressed("jump") && _player.DoubleJumpAvailable)
			{
				stateMachine.TransitionTo("DoubleJumpMovementState");
			}
		}
	}
}
