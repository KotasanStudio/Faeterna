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
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>Al entrar en el estado de doble salto: aplica la velocidad vertical de doble salto.</summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.DoubleJumpAvailable = false;
            _player.SetAnimation("double_jump");
            GD.Print("Entered DoubleJumpMovementState (double jump)");
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.MoveAndSlide();
        }

        public override void Update(double delta)
        {
            if (_player == null) return;
            if (_player.Velocity.Y >= 0)
            {
                GD.Print("Transitioning to falling state from double jump.");
                stateMachine.TransitionTo("FallingMovementState");
            }

            // Cuando la animación de doble salto termina, pasar a la animación de salto normal.
            if (_player.animatedSprite != null && _player.animatedSprite.Frame == 5)
            {
                GD.Print("Double jump animation finished, transitioning to jump animation.");
                _player.SetAnimation("jump");
            }
        }

        /// <summary>Update de física durante el doble salto: aplica gravedad y control horizontal.</summary>
        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;
            if (!_player.IsOnFloor())
            {
                Vector2 velocity = _player.Velocity;
                velocity += _player.GetGravity() * (float)delta;
                float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
                velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
                _player.Velocity = velocity;
                _player.MoveAndSlide();
            }
        }

        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("dash"))
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}