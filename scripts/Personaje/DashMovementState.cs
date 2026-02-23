using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
    public partial class DashMovementState : State
    {
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
                ReturnToMovementState();
                return;
            }

            _player.DashAvailable = false;
            _dashCooldownTimer.Start();

            // Dirección: input → velocidad actual → derecha por defecto.
            _dashDirection = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            if (Mathf.Abs(_dashDirection) < 0.1f)
                _dashDirection = _player.animatedSprite.FlipH ? -1f : 1f;

            _isDashing = true;
            _player.SetAnimation("dash");

            // Dash horizontal puro: sin componente vertical ni de profundidad.
            _player.Velocity = new Vector3(_dashDirection * PlayerType.Speed * 3f, 0f, 0f);
            _player.MoveAndSlide();

            _dashDurationTimer.Start();
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null || !_isDashing) return;

            // Mantener velocidad de dash fija durante el impulso (ignorar gravedad).
            _player.Velocity = new Vector3(_dashDirection * PlayerType.Speed * 3f, 0f, 0f);
            _player.MoveAndSlide();
        }

        /// <summary>No procesamos input durante el dash (invulnerabilidad de frames).</summary>
        public override void HandleInput(InputEvent ev) { }

        public override void Exit()
        {
            _isDashing = false;
            // Guard: Ready() es async, Exit() puede llamarse antes de que los timers estén asignados.
            if (_dashDurationTimer != null && !_dashDurationTimer.IsStopped())
                _dashDurationTimer.Stop();
        }

        private void OnDashFinished()
        {
            _isDashing = false;
            ReturnToMovementState();
        }

        private void OnCooldownFinished()
        {
            if (_player != null)
                _player.DashAvailable = true;
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
                stateMachine.TransitionTo(_player.Velocity.Y > 0f ? "JumpingMovementState" : "FallingMovementState");
            }
        }
    }
}
