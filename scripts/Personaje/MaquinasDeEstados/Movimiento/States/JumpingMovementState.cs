using Godot;
using System;
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
            if (_player == null) return;
            _player.SetAnimation("jump");
            // Aplicar impulso de salto — NO llamar MoveAndSlide aquí.
            // MoveAndSlide resolvería la colisión con el suelo en este mismo frame
            // y anularía el impulso vertical antes de que UpdatePhysics lo procese.
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
        }

        public override void Update(double delta)
        {
            if (_player == null) return;
            // Transición a caída cuando la velocidad vertical se vuelve positiva (bajando).
            if (_player.Velocity.Y > 0)
                stateMachine.TransitionTo("FallingMovementState");
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;

            Vector2 velocity = _player.Velocity;

            // Gravedad siempre activa — sin el if(!IsOnFloor()) para que el impulso
            // del primer frame no quede bloqueado por la colisión del suelo.
            velocity.Y += PlayerType.Gravity * (float)delta;

            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
            _player.Velocity = velocity;

            if (move < 0f)
                _player.FlipH(true);
            else if (move > 0f)
                _player.FlipH(false);

            _player.MoveAndSlide();
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