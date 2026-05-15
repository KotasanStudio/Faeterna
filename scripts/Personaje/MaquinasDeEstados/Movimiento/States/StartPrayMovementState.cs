using Godot;
using System;
using System.Threading.Tasks;
using Faeterna.Scripts.Personaje;
using Faeterna.Scripts.Tools; // Para CheckPoint
using Faeterna.scripts.Mapa;
using Faeterna.Scripts.Menus;  // Para Objeto

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    public partial class StartPrayMovementState : State
    {
        private Lira _player;
        private bool _isInteracting = false;
        private const float ArrivalThreshold = 5.0f;

        // Variables de configuración que llenaremos antes de entrar
        public Vector2 TargetPosition;
        public Node InteractorNode; // Aquí guardaremos el CheckPoint O el Objeto

        public override void Ready()
        {
            _player = (Lira)GetTree().GetFirstNodeInGroup("Lira");
        }

        public override void Enter()
        {
            if (_player == null) return;
            _isInteracting = false;
            
            _player.IsInvulnerableByState = true;
            _player.FlipH(_player.GlobalPosition.X > TargetPosition.X);
        }

        public override void Exit()
        {
            if (_player != null) _player.IsInvulnerableByState = false;
        }

        public override void Update(double delta)
        {
            if (_player == null || _isInteracting) return;

            float distanceX = TargetPosition.X - _player.GlobalPosition.X;

            if (Mathf.Abs(distanceX) > ArrivalThreshold)
            {
                float direction = Mathf.Sign(distanceX);
                _player.Velocity = new Vector2(direction * (Lira.Speed * 0.4f), _player.Velocity.Y);
                _player.AnimTree?.Travel("run");
                _player.MoveAndSlide();
            }
            else
            {
                _player.Velocity = Vector2.Zero;
                ExecuteInteraction();
            }
        }

        private async void ExecuteInteraction()
		{
    		_isInteracting = true;
    		_player.GlobalPosition = new Vector2(TargetPosition.X, _player.GlobalPosition.Y);

    		_player.AnimTree?.Start("startPray");

    		if (InteractorNode is CheckPoint cp)
    		{
    		    await cp.ActionSaveProgress(_player);
    		    await ToSignal(GetTree().CreateTimer(2.8f), "timeout");
    		}
    		else if (InteractorNode is Objeto item)
    		{
    		    await ToSignal(GetTree().CreateTimer(0.6f), "timeout");
    		    int idActual = item.GetItemId(); // Necesitas un getter para la ID
    		    item.Recoger(_player); 
    		    await ToSignal(_player._objectoDescription, ObjetoDescription.SignalName.MenuClosed);
    		    await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
    		}

    		stateMachine.TransitionTo("IdleMovementState");
		}	
    }
}