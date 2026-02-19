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

        /// <summary>Al entrar en Falling: reproducir animación de caída.</summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("fall");
        }

        public override void Update(double delta) { }

        /// <summary>Update de física en Falling: aplica gravedad y control horizontal en aire.</summary>
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

            if (_player.IsOnFloor())
            {
                _player.DoubleJumpAvailable = true;
                GD.Print("Transitioning to idle/running state from falling (landed).");
                stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f
                    ? "RunningMovementState"
                    : "IdleMovementState");
            }
        }

        /// <summary>Procesa eventos de entrada mientras se está en Falling.</summary>
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