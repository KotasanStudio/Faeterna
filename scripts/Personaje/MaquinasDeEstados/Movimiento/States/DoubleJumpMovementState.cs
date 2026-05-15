using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;



namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    /// <summary>
    /// Estado que representa el doble salto de Lira. El jugador realiza un segundo salto mientras está en el aire.
    /// Solo está disponible una vez por carga aérea y se desactiva hasta que el jugador aterrice nuevamente.
    /// Gestiona la aplicación de gravedad y transiciona a otros estados después de completarse la animación del doble salto.
    /// </summary>
    public partial class DoubleJumpMovementState : State
    {
        /// <summary>Referencia al jugador (Lira) para acceder a su estado y controlar su movimiento durante el doble salto.</summary>
        private PlayerType _player;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Obtiene referencias al jugador, esperando a que esté listo si es necesario.
        /// </summary>
        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>
        /// Se llama cuando se entra en este estado. Marca el doble salto como no disponible, establece la animación de doble salto
        /// y aplica la velocidad inicial del salto hacia arriba.
        /// </summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.PlayAudio("jump");
            _player.dobleSaltoParticulas.Emitting = true;
            _player.DoubleJumpAvailable = false;
            _player.SetAnimation("jump");
            GD.Print("Entered DoubleJumpMovementState (double jump)");
            // En 2D, JumpVelocity es negativo (hacia arriba).
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.MoveAndSlide();
        }

        /// <summary>
        /// Se llama cada frame (non-physics). Controla la transición a caída cuando la velocidad vertical se vuelve positiva,
        /// y maneja la transición desde la animación de doble salto a la animación de salto normal cuando termina.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Se pasa al estado para que pueda usarlo en su lógica de actualización.
        /// </param>
        public override void Update(double delta)
        {
            if (_player == null) return;
            // En 2D, Y > 0 significa que estamos cayendo.
            if (_player.Velocity.Y > 0)
            {
                stateMachine.TransitionTo("FallingMovementState");
            }
             if (_player.IsOnFloor())
            {
                if (_player.Velocity.X != 0)
                {
                    stateMachine.TransitionTo("RunningMovementState");
                }
                else if (_player.Velocity.X == 0&&_player.Velocity.Y == 0)
                {
                    stateMachine.TransitionTo("IdleMovementState");
                }
            }
        }

        /// <summary>
        /// Se llama en el paso de física. Aplica gravedad al jugador durante el doble salto, permite movimiento horizontal
        /// y actualiza el sprite según la dirección del movimiento.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde la última actualización de física. Se pasa al estado para su lógica de física.
        /// </param>
        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;
            if (!_player.IsOnFloor())
            {
                Vector2 velocity = _player.Velocity;
                velocity.Y += PlayerType.Gravity * (float)delta;
                float direction = Input.GetAxis("move_left", "move_right");
                if (direction != 0.0f)
                {
                    velocity.X = direction * PlayerType.Speed;
                }
                else
                {
                    velocity.X = Mathf.MoveToward(_player.Velocity.X, 0, PlayerType.Speed);
                }

                if (direction < 0f)
                    _player.FlipH(true);
                else if (direction > 0f)
                    _player.FlipH(false);

                _player.Velocity = velocity;
                _player.MoveAndSlide();
            }
            else // Si estamos en el suelo, aseguramos que la velocidad vertical se restablece a 0 para evitar problemas de física.
            {
                stateMachine.TransitionTo(
                    Mathf.Abs(_player.Velocity.X) > 0.1f
                        ? "RunningMovementState"
                        : "IdleMovementState"
                );
            }
        }

        /// <summary>
        /// Se llama para procesar eventos de entrada no manejados. Solo permite activar el dash durante el doble salto;
        /// el doble salto solo se puede realizar una vez por carga aérea y no se puede hacer otro salto.
        /// </summary>
        /// <param name="ev">
        /// Evento de entrada recibido. Se pasa al estado para que pueda procesar la entrada según su lógica específica.
        /// </param>
        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("dash") && _player.HasDash())
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}
