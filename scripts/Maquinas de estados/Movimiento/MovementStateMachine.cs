using System.Collections.Generic;
using Godot;
using Faeterna.scripts.Maquinas_de_estados;

namespace Faeterna.scripts.Maquinas_de_estados.Movimiento
{
    public partial class MovementStateMachine : Node
    {
        /// <summary>Ruta (NodePath) al estado inicial dentro de este nodo.</summary>
        [Export] public NodePath initialState;

        /// <summary>Mapa de estados indexado por el <c>Node.Name</c> de cada estado.</summary>
        private Dictionary<string, State> _states;

        /// <summary>Estado actualmente activo en la máquina.</summary>
        private State _current_state;

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

        public override void _Process(double delta)
        {
            _current_state.Update(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            _current_state.UpdatePhysics(delta);
        }

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