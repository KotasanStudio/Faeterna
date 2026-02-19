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

            _dashDurationTimer  = GetNode<Timer>("DashDurationTimer");
            _dashCooldownTimer  = GetNode<Timer>("DashCooldownTimer");

            // Cuando el dash termina, si el jugador sigue en el aire dejamos que los estados
            // aéreos tomen el control; si está en el suelo transicionamos allí.
            _dashDurationTimer.Timeout += OnDashFinished;

            // Cuando el cooldown termina volvemos a habilitar el dash.
            _dashCooldownTimer.Timeout += OnCooldownFinished;
        }

        /// <summary>Al entrar: si el dash está disponible lo ejecutamos; si no, volvemos inmediatamente.</summary>
        public override void Enter()
        {
            if (!_player.DashAvailable)
            {
                // Dash en cooldown: volver al estado que corresponda sin hacer nada.
                ReturnToMovementState();
                return;
            }

            // Consumir el dash y arrancar el cooldown.
            _player.DashAvailable = false;
            _dashCooldownTimer.Start();

            // Calcular dirección: input actual → velocidad actual → derecha por defecto.
            _dashDirection = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            if (Mathf.Abs(_dashDirection) < 0.1f)
                _dashDirection = _player.Velocity.X >= 0f ? 1f : -1f;

            _isDashing = true;
            _player.SetAnimation("dash");

            // Aplicar velocidad de dash (sin componente vertical para que sea horizontal puro).
            _player.Velocity = new Vector2(_dashDirection * PlayerType.Speed * 3f, 0f);
            _player.MoveAndSlide();

            // Arrancar el timer que define cuánto dura el impulso.
            _dashDurationTimer.Start();
        }

        /// <summary>
        /// Durante el dash mantenemos la velocidad horizontal fija e ignoramos la gravedad
        /// para que el dash sea siempre horizontal y consistente.
        /// </summary>
        public override void UpdatePhysics(double delta)
        {
            if (!_isDashing)
                return;

            // Mantener velocidad de dash horizontal, sin gravedad durante el impulso.
            _player.Velocity = new Vector2(_dashDirection * PlayerType.Speed * 3f, 0f);
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

        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>Llamado cuando el timer de duración del dash se agota.</summary>
        private void OnDashFinished()
        {
            _isDashing = false;
            ReturnToMovementState();
        }

        /// <summary>Llamado cuando el cooldown termina: el dash vuelve a estar disponible.</summary>
        private void OnCooldownFinished()
        {
            _player.DashAvailable = true;
        }

        /// <summary>
        /// Transiciona al estado de movimiento adecuado según si el jugador está en el suelo o en el aire.
        /// </summary>
        private void ReturnToMovementState()
        {
            if (_player.IsOnFloor())
            {
                bool movingHorizontally = Mathf.Abs(_player.Velocity.X) > 0.1f
                                          && (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"));
                stateMachine.TransitionTo(movingHorizontally ? "RunningMovementState" : "IdleMovementState");
            }
            else
            {
                // El personaje está en el aire: dejar que la física aérea retome el control.
                stateMachine.TransitionTo(_player.Velocity.Y < 0f ? "JumpingMovementState" : "FallingMovementState");
            }
        }
    }
}