using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    public partial class RunningMovementState : State
    {
        private PlayerType _player;
        private Timer _coyoteTimer;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
            _coyoteTimer = GetNode<Timer>("CoyoteTimer");
        }

        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("run");
        }

        public override void Update(double delta)
        {
            if (_player == null || _coyoteTimer == null) return;

            if (!_player.IsOnFloor())
            {
                if (_player.CoyoteAvailable)
                {
                    _coyoteTimer.Start();
                    _player.CoyoteAvailable = false;
                }
                else if (_coyoteTimer.IsStopped())
                {
                    GD.Print("Transitioning to falling state from running.");
                    stateMachine.TransitionTo("FallingMovementState");
                }
            }
            else
            {
                _coyoteTimer.Stop();
                _player.CoyoteAvailable = true;
            }
            float direction = Input.GetAxis("move_left", "move_right");

            if (direction == 0f)
            {
                if (_player.Velocity.X == 0f && _player.Velocity.Y == 0f)
                {
                    GD.Print("Transitioning to idle state from running.");
                    stateMachine.TransitionTo("IdleMovementState");
                }
            }
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;

           Vector2 velocity = _player.Velocity;
        
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

        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("jump"))
            {
                GD.Print("Transitioning to jumping state from running.");
                stateMachine.TransitionTo("JumpingMovementState");
            }
            if (ev.IsActionPressed("dash"))
            {
                GD.Print("Transitioning to dash state from running.");
                stateMachine.TransitionTo("DashMovementState");
            }
            if (ev.IsActionPressed("aim"))
                stateMachine.TransitionTo("MagicMovementState");
            if (ev.IsActionPressed("kick"))
                stateMachine.TransitionTo("AttackMovementState");
        }
    }
}
