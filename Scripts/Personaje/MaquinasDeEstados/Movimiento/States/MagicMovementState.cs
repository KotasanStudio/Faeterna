using Godot;
using System;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
	public partial class MagicMovementState : State
	{
        private PlayerType _player;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("aim");
        }

        public override void Update(double delta)
        {
            if (_player == null) return;

            if (Input.IsActionPressed("shot"))
            {
                _player.Shooting();
            }

            if (Input.IsActionPressed("move_left"))
            {
                _player.FlipH(true);
            }

            if (Input.IsActionPressed("move_right"))
            {
                _player.FlipH(false);
            }
        }

        public override async void HandleInput(InputEvent ev)
        {
            if (_player == null) return;
            if (ev.IsActionPressed("aim"))
            {
                _player.animatedSprite.PlayBackwards("aim");
                await ToSignal(_player.animatedSprite, "animation_finished");
                stateMachine.TransitionTo("IdleMovementState");
            }
        }
    }
}
