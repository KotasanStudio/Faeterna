using Godot;
using System;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
	public partial class MagicMovementState : State
	{
        private PlayerType _player;
        private double _maxShotBallScale = 4f;
        private double _defaultManaCost = 20f;
        private double _maxManaCost = 100f;
        private double _manaCost = 20f;
        private bool _isShooted = false;
        private double _defaultShotBallScale = 1.5f;
                private double _shotBallScale = 1.5f;
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
                    _player.PlayAudio("fireball");
                if(_shotBallScale < _maxShotBallScale && _manaCost < _maxManaCost)
                {
                    _manaCost += _manaCost*delta; // Ajusta el costo de mana por segundo
                 _shotBallScale += _shotBallScale * delta;// Limita el tamaño máximo de la bola
                } 
                else
                {
                    _player.Shooting(_manaCost, _shotBallScale);
                    resetShotParameters();
                    _isShooted = true;
                }
                 
            }

            if (Input.IsActionJustReleased("shot")&& !_isShooted)
            {      
                    _player.Shooting(_manaCost, _shotBallScale);
                    resetShotParameters();
            }

            if (Input.IsActionPressed("move_left"))
            {
                _player.FlipH(true);
            }

            if (Input.IsActionPressed("move_right"))
            {
                _player.FlipH(false);
            }

            if (Input.IsActionJustReleased("aim"))
            {
                stateMachine.TransitionTo("IdleMovementState");
            }
            _isShooted = false;
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

        private void resetShotParameters()
        {
            _manaCost = _defaultManaCost;
            _shotBallScale = _defaultShotBallScale;
        }
    }
}
