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

        /// <summary>Al inicio del salto: aplicar la velocidad vertical de salto.</summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("jump");
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.MoveAndSlide();
        }

        /// <summary>Actualización por frame durante el salto: gestiona transición a caída o al suelo.</summary>
        public override void Update(double delta)
        {
            if (_player == null) return;
            if (_player.Velocity.Y >= 0)
            {
                GD.Print("Transitioning to falling state from jumping.");
                stateMachine.TransitionTo("FallingMovementState");
            }

            if (_player.IsOnFloor())
            {
                GD.Print("Transitioning to idle/running state from jumping (landed).");
                stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f
                    ? "RunningMovementState"
                    : "IdleMovementState");
            }
        }

        /// <summary>Update de física durante el salto: aplica gravedad y control horizontal.</summary>
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
            if (ev.IsActionPressed("jump") && _player.DoubleJumpAvailable)
                stateMachine.TransitionTo("DoubleJumpMovementState");
            if (ev.IsActionPressed("dash"))
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}