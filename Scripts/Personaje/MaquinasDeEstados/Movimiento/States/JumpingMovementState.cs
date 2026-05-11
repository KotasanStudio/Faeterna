using System;
using Godot;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
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

        public override void Enter()
        {
            if (_player == null)
                return;

            _player.SetAnimation("jump");
            // En 2D, JumpVelocity es negativo (hacia arriba).
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.MoveAndSlide();
        }

        public override void Update(double delta)
        {
            if (_player == null)
                return;
            // En 2D, Y > 0 significa que estamos cayendo (bajando).
            if (_player.Velocity.Y > 0)
            {
                GD.Print("Transitioning to falling state from jumping.");
                stateMachine.TransitionTo("FallingMovementState");
            }
            if (_player.IsOnFloor())
            {
                GD.Print("Transitioning to idle/running state from jumping (landed).");
                stateMachine.TransitionTo(
                    Mathf.Abs(_player.Velocity.X) > 0.1f
                        ? "RunningMovementState"
                        : "IdleMovementState"
                );
            }
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null)
                return;
            if (!_player.IsOnFloor())
            {
                Vector2 velocity = _player.Velocity;
                // Aplicar gravedad: en 2D sumamos la gravedad positiva (hacia abajo).
                velocity.Y += PlayerType.Gravity * (float)delta;
                float move =
                    Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
                velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
                _player.Velocity = velocity;

                if (move < 0f)
                    _player.FlipH(true);
                else if (move > 0f)
                    _player.FlipH(false);

                _player.MoveAndSlide();
            }
        }

        public override void HandleInput(InputEvent ev)
        {
            if (_player == null)
                return;
            if (ev.IsActionPressed("jump") && _player.DoubleJumpAvailable)
                stateMachine.TransitionTo("DoubleJumpMovementState");
            if (ev.IsActionPressed("dash"))
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}
