using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    /// <summary>
    /// Estado que representa el movimiento de dash (impulso rápido horizontal) de Lira.
    /// </summary>
    public partial class DashMovementState : State
    {
        /// <summary>Referencia al jugador (Lira) para acceder a su estado y controlar su movimiento durante el dash.</summary>
        private PlayerType _player;

        /// <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>
        private Timer _dashDurationTimer;

        /// <summary>Timer que controla el cooldown antes de poder volver a hacer dash.</summary>
        private Timer _dashCooldownTimer;

        /// <summary>Dirección horizontal del dash (1 = derecha, -1 = izquierda).</summary>
        private float _dashDirection;

        /// <summary>Indica si el dash sigue activo (en impulso) o ya terminó y esperamos transición.</summary>
        private bool _isDashing;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");

            _dashDurationTimer = GetNode<Timer>("DashDurationTimer");
            _dashCooldownTimer = GetNode<Timer>("DashCooldownTimer");

            _dashDurationTimer.Timeout += OnDashFinished;
            _dashCooldownTimer.Timeout += OnCooldownFinished;
        }

        public override void Enter()
        {
            if (_player == null) return;

            if (!_player.DashAvailable)
            {
                if (_player == null) return;
                stateMachine.TransitionTo("IdleMovementState");
                return;
            }

            _player.DashAvailable = false;
            _dashCooldownTimer.Start();

            // Dirección: input → sprite flip → derecha por defecto.
            _dashDirection = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            if (Mathf.Abs(_dashDirection) < 0.1f)
                _dashDirection = _player.animatedSprite.FlipH ? -1f : 1f;

            _isDashing = true;
            _player.SetAnimation("dash");

            // Dash horizontal puro: sin componente vertical.
            _player.Velocity = new Vector2(_dashDirection * PlayerType.Speed * 3f, 0f);
            _player.MoveAndSlide();

            _dashDurationTimer.Start();
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null || !_isDashing) return;

            // Mantener velocidad de dash fija durante el impulso (ignorar gravedad).
            _player.Velocity = new Vector2(_dashDirection * PlayerType.Speed * 3f, 0f);
            _player.MoveAndSlide();
        }

        /// <summary>No procesamos input durante el dash (invulnerabilidad de frames).</summary>
        public override void HandleInput(InputEvent ev) { }

        public override void Exit()
        {
            _isDashing = false;
            if (_dashDurationTimer != null && !_dashDurationTimer.IsStopped())
                _dashDurationTimer.Stop();
        }

        private void OnDashFinished()
        {
            _isDashing = false;
            if (_player == null)
                return;
            stateMachine.TransitionTo("IdleMovementState");
        }

        private void OnCooldownFinished()
        {
            if (_player != null)
                _player.DashAvailable = true;
        }
    }
}
