using System.Collections.Generic;
using Godot;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento
{
    /// <summary>
    /// Máquina de estados que controla el movimiento de Lira.
    /// </summary>
    public partial class MovementStateMachine : Node
    {
        /// <summary>Ruta (NodePath) al estado inicial dentro de este nodo.</summary>
        [Export] public NodePath initialState;

        /// <summary>Mapa de estados indexado por el <c>Node.Name</c> de cada estado.</summary>
        private Dictionary<string, State> _states;

        /// <summary>Estado actualmente activo en la máquina.</summary>
        private State _current_state;

        /// <summary>
        /// Se llama cuando el nodo entra en la escena. Inicializa el mapa de estados y establece el estado actual al estado inicial exportado.
        /// </summary>
        public override void _Ready()
        {
            _states = new Dictionary<string, State>();
            foreach (Node node in GetChildren())
            {
                if (node is State s)
                {
                    _states[node.Name] = s;
                    s.stateMachine = this;
                    s.Ready();
                    s.Exit();
                }
            }

            // Inicializar el estado actual a partir de la ruta exportada.
            _current_state = GetNode<State>(initialState);
            _current_state.Enter();
        }

        /// <summary>
        /// Se llama cada frame (non-physics). Delegamos la actualización al estado actual.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde el último frame. Se pasa al estado actual para que pueda usarlo en su lógica de actualización.
        /// </param>
        public override void _Process(double delta)
        {
            _current_state.Update(delta);
        }

        /// <summary>
        /// Se llama en el paso de física. Delegamos la actualización de física al estado actual.
        /// </summary>
        /// <param name="delta">
        /// Tiempo en segundos desde la última actualización de física. Se pasa al estado actual para que pueda usarlo en su lógica de física.
        /// </param>
        public override void _PhysicsProcess(double delta)
        {
            _current_state.UpdatePhysics(delta);
        }

        /// <summary>
        /// Se llama para procesar eventos de entrada no manejados. Delegamos el manejo de entrada al estado actual.
        /// </summary>
        /// <param name="event">
        /// Evento de entrada recibido. Se pasa al estado actual para que pueda procesar la entrada según su lógica específica.
        /// </param>
        public override void _UnhandledInput(InputEvent @event)
        {
            _current_state.HandleInput(@event);
        }

        /// <summary>
        /// Pide la transición a otro estado por su clave (nombre del nodo).
        /// Si la clave no existe o ya estamos en ese estado la petición se ignora.
        /// </summary>
        /// <param name="key">Nombre del nodo-estado destino dentro de este nodo.</param>
        public void TransitionTo(string key)
        {
            if (!_states.ContainsKey(key) || _current_state == _states[key])
            {
                // Ignorar si la clave no existe o ya es el estado actual.
                return;
            }

            _current_state.Exit();
            _current_state = _states[key];
            _current_state.Enter();
        }
    }
}
