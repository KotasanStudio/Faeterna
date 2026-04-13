using Godot;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    /// <summary>
    /// Estado activo durante el knockback al recibir daño.
    /// Bloquea el input del jugador y aplica la velocidad de golpe
    /// hasta que el timer expira o el personaje toca el suelo.
    /// </summary>
    public partial class KnockbackMovementState : State
    {
        private PlayerType _player;

        /// <summary>Timer que limita cuánto tiempo dura el bloqueo de input.</summary>
        private Timer _knockbackTimer;

        /// <summary>Indica si el knockback sigue activo.</summary>
        private bool _isKnockbackActive;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");

            _knockbackTimer = GetNode<Timer>("KnockbackTimer");
            _knockbackTimer.Timeout += OnKnockbackFinished;
        }

        public override void Enter()
        {
            if (_player == null) return;

            _isKnockbackActive = true;
            _player.SetAnimation("getHit");
            _knockbackTimer.Start();
        }

        public override void UpdatePhysics(double delta)
        {
            if (_player == null || !_isKnockbackActive) return;

            Vector2 velocity = _player.Velocity;

            // Aplicar gravedad para que el arco del salto sea natural
            velocity.Y += PlayerType.Gravity * (float)delta;

            // Decelerar gradualmente el impulso horizontal (fricción)
            velocity.X = Mathf.MoveToward(velocity.X, 0f, PlayerType.Speed * (float)delta * 3f);

            _player.Velocity = velocity;
            _player.MoveAndSlide();
        }

        /// <summary>Bloqueamos todo el input durante el knockback.</summary>
        public override void HandleInput(InputEvent ev) { }

        public override void Exit()
        {
            _isKnockbackActive = false;
            if (_knockbackTimer != null && !_knockbackTimer.IsStopped())
                _knockbackTimer.Stop();
        }

        private void OnKnockbackFinished()
        {
            _isKnockbackActive = false;
            ReturnToMovementState();
        }

        private void ReturnToMovementState()
        {
            if (_player == null) return;
            if (_player.IsOnFloor())
                stateMachine.TransitionTo("IdleMovementState");
            else
                stateMachine.TransitionTo(_player.Velocity.Y < 0f ? "JumpingMovementState" : "FallingMovementState");
        }
    }
}
