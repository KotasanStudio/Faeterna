using Godot;
using System;
using PlayerType = Faeterna.Scripts.Personaje.Lira;
using Faeterna.Scripts.Tools;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    public partial class StartPrayMovementState : State
    {
        private PlayerType _player;
        public Vector2 TargetPosition;
        public CheckPoint CurrentCheckPoint;
        private bool _isPraying = false;
        private const float ArrivalThreshold = 5.0f;

        public override void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
        }

        public override void Enter()
		{
		    if (_player == null) return;
		    _isPraying = false;
		    _player.IsInvulnerableByState = true;
		    _player.FlipH(_player.GlobalPosition.X > TargetPosition.X);
		}
		public override void Exit()
		{
		    if (_player != null)
		    {
		        _player.IsInvulnerableByState = false;
		    }
		}

        public override void Update(double delta)
        {
            if (_player == null || _isPraying) return;

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
                ExecutePraySequence();
            }
        }

        private async void ExecutePraySequence()
		{
		    _isPraying = true;
		    _player.GlobalPosition = new Vector2(TargetPosition.X, _player.GlobalPosition.Y);
		    _player.AnimTree?.Start("startPray");
		    if (CurrentCheckPoint != null)
		    {
		        await CurrentCheckPoint.ActionSaveProgress(_player);
		    }
		    await ToSignal(GetTree().CreateTimer(2f), "timeout");
		    stateMachine.TransitionTo("IdleMovementState");
		}
	}
}