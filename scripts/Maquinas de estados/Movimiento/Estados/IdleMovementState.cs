using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
    /// <summary>
    /// Estado que representa el reposo/inactividad de Lira. El jugador está parado en el suelo sin moverse.
    /// Este estado es el estado inicial y gestiona las transiciones a correr, saltar o caer cuando el jugador realiza acciones.
    /// </summary>
    public partial class IdleMovementState : State
    {
        /// <summary>Referencia al jugador (Lira) para acceder a su estado y controlar su animación durante el reposo.</summary>
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
        /// Se llama cuando se entra en este estado. Establece la animación de reposo en el jugador y aplica el movimiento base.
        /// </summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("idle");
            _player.MoveAndSlide();
        }

        /// <summary>
        /// Se llama cada frame (non-physics). Controla la transición del jugador a otros estados cuando deja de estar en el suelo (salta o cae).
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Se pasa al estado para que pueda usarlo en su lógica de actualización, aunque en este caso no se utiliza directamente.
        /// </param>
        public override void Update(double delta)
        {
            if (_player == null) return;
            if (!_player.IsOnFloor())
            {
                GD.Print("Transitioning to falling or jumping state from idle.");
                // En 2D, Y positivo = hacia abajo, negativo = hacia arriba (subiendo).
                stateMachine.TransitionTo(_player.Velocity.Y < 0
                    ? "JumpingMovementState"
                    : "FallingMovementState");
            }
        }

        /// <summary>
        /// Se llama para procesar eventos de entrada no manejados. Controla la transición a otros estados según la entrada del jugador, como moverse, saltar o dash.
        /// Si el jugador presiona las teclas de movimiento, transiciona a correr; si presiona salto, transiciona a saltar; si presiona dash, transiciona a dash.
        /// </summary>
        /// <param name="ev">
        /// Evento de entrada recibido. Se pasa al estado para que pueda procesar la entrada según su lógica específica.
        /// </param>
        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("move_left") || ev.IsActionPressed("move_right"))
                stateMachine.TransitionTo("RunningMovementState");
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
            if (ev.IsActionPressed("dash"))
                stateMachine.TransitionTo("DashMovementState");
        }
    }
}
