using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
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

            if (Mathf.Abs(_player.Velocity.X) < 0.1f)
            {
                if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
                {
                    GD.Print("Transitioning to idle state from running.");
                    stateMachine.TransitionTo("IdleMovementState");
                }
            }
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;

            // En 3D el movimiento horizontal sigue siendo en X.
            // Flip del sprite según dirección para side-scroller.
            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");

            Vector3 velocity = _player.Velocity;
            velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
            _player.Velocity = velocity;

            if (move < 0f)
                _player.animatedSprite.FlipH = true;
            else if (move > 0f)
                _player.animatedSprite.FlipH = false;

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
        }
    }
}
