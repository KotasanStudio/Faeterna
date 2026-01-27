using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
	public partial class JumpingMovementState : State
	{
		private PlayerType _player;

		public override async void Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}

		/// <summary>Acciones al inicio del salto: aplicar la velocidad vertical de salto.</summary>
		public override void Enter()
		{
			_player.SetAnimation("jump");

            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.MoveAndSlide();
		}

		/// <summary>Actualización por frame durante el salto: gestiona transición a caída o al suelo.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void Update(double delta)
		{
			if (_player.Velocity.Y >= 0)
			{
				GD.Print("Transitioning to falling state from jumping.");
				stateMachine.TransitionTo("FallingMovementState");
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
		
		/// <summary>Update de física durante el salto. Implementa control horizontal inmediato (stop si no hay input).</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void UpdatePhysics(double delta)
		{
			if (!_player.IsOnFloor())
			{
				Vector2 velocity = _player.Velocity;
				velocity += _player.GetGravity() * (float)delta;
				// Horizontal air control tipo Hollow Knight: detenerse inmediatamente si no hay input
				float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
				if (Mathf.Abs(move) > 0f)
					velocity.X = move * PlayerType.Speed;
				else
					velocity.X = 0f;
				_player.Velocity = velocity;
				_player.MoveAndSlide();
			}
		}

		public override void HandleInput(InputEvent ev)
		{
			if (ev.IsActionPressed("jump") && _player.DoubleJumpAvailable)
				stateMachine.TransitionTo("DoubleJumpMovementState");
		}
	}
}
