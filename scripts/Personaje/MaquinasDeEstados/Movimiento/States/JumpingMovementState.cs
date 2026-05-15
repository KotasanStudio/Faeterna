using System;
using Godot;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    /// <summary>
    /// Estado que representa el movimiento de salto de Lira. El jugador está en el aire después de haber saltado desde el suelo.
    /// Gestiona la aplicación de gravedad y las transiciones a caída, doble salto o aterrizaje.
    /// </summary>
    public partial class JumpingMovementState : State
    {
        /// <summary>Referencia al jugador (Lira) para acceder a su estado y controlar su movimiento durante el salto.</summary>
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
        /// Se llama cuando se entra en este estado. Establece la animación de salto en el jugador y aplica la velocidad inicial del salto hacia arriba.
        /// </summary>
        public override void Enter()
        {
            if (_player == null)
                return;

            _player.SetAnimation("jump");
            // En 2D, JumpVelocity es negativo (hacia arriba).
            _player.Velocity = new Vector2(_player.Velocity.X, PlayerType.JumpVelocity);
            _player.saltoParticulas.Emitting = true;
            _player.MoveAndSlide();
        }

        /// <summary>
        /// Se llama cada frame (non-physics). Controla la transición a caída cuando la velocidad vertical se vuelve positiva (comienza a bajar),
        /// y detecta si el jugador ha aterrizando al tocar el suelo.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Se pasa al estado para que pueda usarlo en su lógica de actualización, aunque en este caso no se utiliza directamente.
        /// </param>
        public override void Update(double delta)
        {
            if (_player == null)
                return;
            // En 2D, Y > 0 significa que estamos cayendo (bajando).
            if (_player.Velocity.Y > 0)
            {
                GD.Print("Transitioning to falling state from jumping.");
                stateMachine.TransitionTo("FallingMovementState");
            }
            if (_player.IsOnFloor())
            {
                GD.Print("Transitioning to idle/running state from jumping (landed).");
                stateMachine.TransitionTo(
                    Mathf.Abs(_player.Velocity.X) > 0.1f
                        ? "RunningMovementState"
                        : "IdleMovementState"
                );
            }
        }

        /// <summary>
        /// Se llama en el paso de física. Aplica gravedad al jugador, permite el movimiento horizontal durante el salto y actualiza el sprite según la dirección del movimiento.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde la última actualización de física. Se pasa al estado para que pueda usarlo en su lógica de física.
        /// </param>
        public override void UpdatePhysics(double delta)
        {
            if (_player == null)
                return;
            if (!_player.IsOnFloor())
            {
                Vector2 velocity = _player.Velocity;
                // Aplicar gravedad: en 2D sumamos la gravedad positiva (hacia abajo).
                velocity.Y += PlayerType.Gravity * (float)delta;
                float move =
                    Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
                velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
                _player.Velocity = velocity;

                if (move < 0f)
                    _player.FlipH(true);
                else if (move > 0f)
                    _player.FlipH(false);

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
        /// Se llama para procesar eventos de entrada no manejados. Permite realizar un doble salto si está disponible, o activar el dash durante el salto.
        /// </summary>
        /// <param name="ev">
        /// Evento de entrada recibido. Se pasa al estado para que pueda procesar la entrada según su lógica específica.
        /// </param>
        public override void HandleInput(InputEvent ev)
        {
            if (_player == null)
                return;
            if (ev.IsActionPressed("jump") && _player.DoubleJumpAvailable)
                stateMachine.TransitionTo("DoubleJumpMovementState");
            if (ev.IsActionPressed("dash"))
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}
