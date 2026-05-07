using Godot;
using System;
using PlayerType = Faeterna.scripts.Player.Lira;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento.Estados
{
    /// <summary>
    /// Estado que representa el movimiento de correr de Lira.
    /// </summary>
    public partial class RunningMovementState : State
    {
        /// <summary>Referencia al jugador (Lira) para acceder a su estado y controlar su movimiento durante la carrera.</summary>
        private PlayerType _player;

        /// <summary>Timer que controla el tiempo de coyote (coyote time) para permitir saltar poco después de salir del suelo.</summary>
        private Timer _coyoteTimer;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Obtiene referencias al jugador y al timer de coyote, esperando a que el jugador esté listo si es necesario.
        /// </summary>
        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
            _coyoteTimer = GetNode<Timer>("CoyoteTimer");
        }

        /// <summary>
        /// Se llama cuando se entra en este estado. Establece la animación de correr en el jugador.
        /// </summary>
        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("run");
        }

        /// <summary>
        /// Se llama cada frame (non-physics). Controla la lógica de transición a otros estados según el estado del jugador (si está en el aire, si dejó de moverse, etc.) y maneja el timer de coyote.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Se pasa al estado para que pueda usarlo en su lógica de actualización, aunque en este caso no se utiliza directamente.
        /// </param>
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

            if (Mathf.Abs(_player.Velocity.X) < 0.1f)
            {
                if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
                {
                    GD.Print("Transitioning to idle state from running.");
                    stateMachine.TransitionTo("IdleMovementState");
                }
            }
        }

        /// <summary>
        /// Se llama en el paso de física. Controla el movimiento horizontal del jugador según la entrada de movimiento, aplicando la velocidad de carrera y manejando el flip del sprite según la dirección. También aplica el movimiento con MoveAndSlide.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde la última actualización de física. Se pasa al estado para que pueda usarlo en su lógica de física, aunque en este caso no se utiliza directamente.
        /// </param>
        public override void UpdatePhysics(double delta)
        {
            if (_player == null) return;

            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");

            Vector2 velocity = _player.Velocity;
            velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;
            _player.Velocity = velocity;

            if (move < 0f)
                _player.animatedSprite.FlipH = true;
            else if (move > 0f)
                _player.animatedSprite.FlipH = false;

            _player.MoveAndSlide();
        }

        /// <summary>
        /// Se llama para procesar eventos de entrada no manejados. Controla la transición a otros estados según la entrada del jugador, como saltar o dash. Si se presiona el botón de salto, se transiciona al estado de salto; si se presiona el botón de dash, se transiciona al estado de dash.
        /// </summary>
        /// <param name="ev">
        /// Evento de entrada recibido. Se pasa al estado para que pueda procesar la entrada según su lógica específica, como detectar acciones de salto o dash.
        /// </param>
        public override void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("jump"))
            {
                GD.Print("Transitioning to jumping state from running.");
                stateMachine.TransitionTo("JumpingMovementState");
            }
            if (ev.IsActionPressed("dash"))
            {
                GD.Print("Transitioning to dash state from running.");
                stateMachine.TransitionTo("DashMovementState");
            }
        }
    }
}
