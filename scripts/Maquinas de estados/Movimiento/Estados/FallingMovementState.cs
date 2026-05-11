using Godot;
using System;
<<<<<<<< HEAD:scripts/Personaje/MaquinasDeEstados/Movimiento/States/FallingMovementState.cs
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;
========
using PlayerType = Faeterna.scripts.Personaje.Lira;
>>>>>>>> origin/Particulas:scripts/Maquinas de estados/Movimiento/Estados/FallingMovementState.cs

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    /// <summary>
    /// Estado que representa la caída de Lira. El jugador está en el aire cayendo hacia el suelo.
    /// Gestiona la aplicación de gravedad continuada, permite aún el movimiento horizontal durante la caída,
    /// y detecta el aterrizaje para transicionar a otros estados. También permite el doble salto durante la caída.
    /// </summary>
    public partial class FallingMovementState : State
    {
        /// <summary>Referencia al jugador (Lira) para acceder a su estado y controlar su movimiento durante la caída.</summary>
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
        /// Se llama cuando se entra en este estado. Establece la animación de caída en el jugador.
        /// </summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("fall");
        }

        /// <summary>
        /// Se llama cada frame (non-physics). No realiza actualizaciones específicas en este estado,
        /// las comprobaciones de aterrizaje y transición se hacen en UpdatePhysics.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame.
        /// </param>
        public override void Update(double delta) { }

        /// <summary>
        /// Se llama en el paso de física. Aplica gravedad continuada al jugador, permite movimiento horizontal durante la caída,
        /// detecta el aterrizaje y transiciona a otros estados cuando toca el suelo.
        /// También restaura la disponibilidad del doble salto cuando aterriza.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde la última actualización de física.
        /// </param>
        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;

            if (!_player.IsOnFloor())
            {
                Vector2 velocity = _player.Velocity;
                // Aplicar gravedad: en 2D sumamos la gravedad positiva (hacia abajo).
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

            if (_player.IsOnFloor())
            {
                _player.DoubleJumpAvailable = true;
                GD.Print("Transitioning to idle/running state from falling (landed).");
                stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f
                    ? "RunningMovementState"
                    : "IdleMovementState");
            }
        }

        /// <summary>
        /// Se llama para procesar eventos de entrada no manejados. Permite realizar un doble salto durante la caída,
        /// o activar el dash para cambiar de dirección o velocidad.
        /// </summary>
        /// <param name="ev">
        /// Evento de entrada recibido. Se pasa al estado para que pueda procesar la entrada según su lógica específica.
        /// </param>
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
