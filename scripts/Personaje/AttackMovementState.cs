using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
    public partial class AttackMovementState : State
    {
        private PlayerType _player;

        /// <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>
        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        public override void Enter()
        {
            _player.SetAnimation("attack");
        }

        public override void UpdatePhysics(double delta)
        {
            _player.MoveAndSlide();
        }

        public override void HandleInput(InputEvent ev) { }

        public override void Exit()
        {
        }

        private void ReturnToMovementState()
        {
            if (_player == null) return;
            if (_player.IsOnFloor())
            {
                bool movingHorizontally = Mathf.Abs(_player.Velocity.X) > 0.1f
                    && (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"));
                stateMachine.TransitionTo(movingHorizontally ? "RunningMovementState" : "IdleMovementState");
            }
            else
            {
                stateMachine.TransitionTo(_player.Velocity.Y < 0f ? "JumpingMovementState" : "FallingMovementState");
            }
        }
    }
}