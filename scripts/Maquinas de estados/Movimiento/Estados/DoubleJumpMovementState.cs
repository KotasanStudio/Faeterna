using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
	public partial class DoubleJumpMovementState : State
	{
		private PlayerType _player;

		public override async void Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("NinjaFrogGroup");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}
		
		/// <summary>Al entrar en el estado de doble salto aplica la velocidad vertical de doble salto.</summary>
		public override void Enter()
		{
			_player.DoubleJumpAvailable = false;
			_player.SetAnimation("double_jump");

			GD.Print("Entered DoobleJumpMovementState (double jump)");

			_player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
			_player.MoveAndSlide();
		}

		public override void Update(double delta)
		{
			if (_player.Velocity.Y >= 0)
			{
				GD.Print("Transitioning to falling state from jumping.");
				stateMachine.TransitionTo("FallingMovementState");
			}

			if (_player.animatedSprite.Frame == 5)
			{
				GD.Print("Double jump animation finished, transitioning to falling state.");
				_player.SetAnimation("jump");
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
	}
}
