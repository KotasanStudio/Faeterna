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
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("NinjaFrogGroup");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }
        
        /// <summary>
        /// Al entrar en el estado Idle: establece la animación y detiene la velocidad horizontal.
        /// También resetea el doble salto porque estamos en suelo.
        /// </summary>
        public override void Enter()
        {
            _player.DoubleJumpAvailable = true;
            _player.SetAnimation("idle");
            _player.Velocity = new Vector2(0, _player.Velocity.Y);
            _player.MoveAndSlide();
        }

        /// <summary>Actualización por frame del estado Idle: decide transiciones en base a input/physics.</summary>
        /// <param name="delta">Delta en segundos desde el último frame.</param>
        public override void Update(double delta)
        {
            if (!_player.IsOnFloor())
            {
                GD.Print("Transitioning to falling or jumping state from idle.");
                if (_player.Velocity.Y < 0)
                    stateMachine.TransitionTo("JumpingMovementState");
                else
                    stateMachine.TransitionTo("FallingMovementState");
            }
            
            if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
            {
                GD.Print("Transitioning to running state from idle (input polling).");
                stateMachine.TransitionTo("RunningMovementState");
                return;
            }

            if (Mathf.Abs(_player.Velocity.X) > 0)
            {
                GD.Print("Transitioning to running state from idle (velocity). ");
                stateMachine.TransitionTo("RunningMovementState");
            }
        }

        /// <summary>Procesa eventos de entrada no manejados cuando estamos en Idle.</summary>
        /// <param name="ev">Evento de entrada recibido.</param>
        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("move_left") || ev.IsActionPressed("move_right"))
                stateMachine.TransitionTo("RunningMovementState");
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
        }
    }
}
