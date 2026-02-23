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

        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("fall");
        }

        public override void Update(double delta) { }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;

            if (!_player.IsOnFloor())
            {
                Vector3 velocity = _player.Velocity;
                // Gravedad manual en 3D.
                velocity.Y += PlayerType.Gravity * (float)delta;
                float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
                velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
                velocity.Z = 0f; // Side-scroller: sin profundidad de movimiento.
                _player.Velocity = velocity;

                if (move < 0f)
                    _player.animatedSprite.FlipH = true;
                else if (move > 0f)
                    _player.animatedSprite.FlipH = false;
                
                _player.MoveAndSlide();
            }

            if (_player.IsOnFloor())
            {
                _player.DoubleJumpAvailable = true;
                GD.Print("Transitioning to idle/running state from falling (landed).");
                stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f
                    ? "RunningMovementState"
                    : "IdleMovementState");
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
