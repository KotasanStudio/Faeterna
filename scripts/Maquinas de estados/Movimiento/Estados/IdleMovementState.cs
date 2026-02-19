using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
    public partial class IdleMovementState : State
    {
        private PlayerType _player;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>Al entrar en el estado Idle: establece la animación.</summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("idle");
            _player.MoveAndSlide();
        }

        /// <summary>Actualización por frame del estado Idle: decide transiciones en base a input/physics.</summary>
        public override void Update(double delta)
        {
            if (_player == null) return;
            if (!_player.IsOnFloor())
            {
                GD.Print("Transitioning to falling or jumping state from idle.");
                if (_player.Velocity.Y < 0)
                    stateMachine.TransitionTo("JumpingMovementState");
                else
                    stateMachine.TransitionTo("FallingMovementState");
            }
        }

        /// <summary>Procesa eventos de entrada no manejados cuando estamos en Idle.</summary>
        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("move_left") || ev.IsActionPressed("move_right"))
                stateMachine.TransitionTo("RunningMovementState");
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
            if (ev.IsActionPressed("dash"))
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}