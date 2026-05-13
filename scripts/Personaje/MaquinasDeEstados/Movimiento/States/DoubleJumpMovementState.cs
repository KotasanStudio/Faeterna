using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
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

        public override void Enter()
        {
            if (_player == null) return;
            _player.DoubleJumpAvailable = false;
            _player.SetAnimation("jump");
            GD.Print("Entered DoubleJumpMovementState (double jump)");
            // En 2D, JumpVelocity es negativo (hacia arriba).
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.MoveAndSlide();
        }

        public override void Update(double delta)
        {
            if (_player == null) return;
            // En 2D, Y > 0 significa que estamos cayendo.
            if (_player.Velocity.Y > 0)
            {
                stateMachine.TransitionTo("FallingMovementState");
            }
             if (_player.IsOnFloor())
            {
                if (_player.Velocity.X != 0)
                {
                    stateMachine.TransitionTo("RunningMovementState");  
                }
                else if (_player.Velocity.X == 0&&_player.Velocity.Y == 0)
                {
                    stateMachine.TransitionTo("IdleMovementState");
                }
            }
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;
            if (!_player.IsOnFloor())
            {
                Vector2 velocity = _player.Velocity;
                velocity.Y += PlayerType.Gravity * (float)delta;
                float direction = Input.GetAxis("move_left", "move_right");
            if (direction != 0.0f)
            {
                velocity.X = direction * PlayerType.Speed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(_player.Velocity.X, 0, PlayerType.Speed);
            }

            if (direction < 0f)
                _player.FlipH(true);
            else if (direction > 0f)
                _player.FlipH(false);

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
