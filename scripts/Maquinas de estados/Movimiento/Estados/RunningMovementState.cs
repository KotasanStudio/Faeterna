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

        /// <summary>Al entrar en Running: reproducir animación de carrera.</summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("run");
        }

        /// <summary>Actualización por frame del estado Running: controla transiciones por physics o input.</summary>
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

            if (Mathf.Abs((float)_player.Velocity.X) < 0.1f)
            {
                bool inputPressed = Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right");
                if (!inputPressed)
                {
                    GD.Print("Transitioning to idle state from running.");
                    stateMachine.TransitionTo("IdleMovementState");
                }
            }
        }

        /// <summary>Update de física en Running: aplica la velocidad horizontal en función del input.</summary>
        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;
            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            if (Mathf.Abs(move) > 0f)
                _player.Velocity = new Vector2(move * PlayerType.Speed, _player.Velocity.Y);
            else
                _player.Velocity = new Vector2(0, _player.Velocity.Y);
            _player.MoveAndSlide();
        }

        /// <summary>Procesa input no manejado en Running.</summary>
        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("jump"))
            {
                stateMachine.TransitionTo("JumpingMovementState");
                GD.Print("Transitioning to jumping state from running.");
            }
            if (ev.IsActionPressed("dash"))
            {
                stateMachine.TransitionTo("DashMovementState");
                GD.Print("Transitioning to dash state from running.");
            }
        }
    }
}