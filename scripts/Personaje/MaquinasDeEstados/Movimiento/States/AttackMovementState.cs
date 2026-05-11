using Godot;
using System;
using Faeterna.Scripts.Personaje.MaquinasDeEstados;
using PlayerType = Faeterna.Scripts.Personaje.Lira;

namespace Faeterna.Scripts.Personaje.MaquinasDeEstados.Movimiento.States
{
    public partial class AttackMovementState : State
    {
        private PlayerType _player;
        private Area2D _attackHitBox;

        public override async void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");

            _attackHitBox = _player.GetNode<Area2D>("KickHitbox");
            SetHitBoxActive(false);

            _player.animatedSprite.FrameChanged += OnFrameChanged;
        }

        public override void Enter()
        {
            if (_player == null) return;
            _player.SetAnimation("kick");
            
            // Cuando termina la animación vuelve a idle
            _player.animatedSprite.AnimationFinished += OnAnimationFinished;
        }

        public override void Exit()
        {
            if (_player == null) return;
            SetHitBoxActive(false);
            if (_player.animatedSprite != null)
            {
                _player.animatedSprite.AnimationFinished -= OnAnimationFinished;
            }
        }

        public override void HandleInput(InputEvent ev) { }

        public override void Update(double delta) { }

        private void OnFrameChanged()
        {
            if (_player.animatedSprite.Animation != "kick") return;

            int frame = _player.animatedSprite.Frame;
            SetHitBoxActive(frame == 2 || frame == 3);
        }

        private void OnAnimationFinished()
        {
            if (_player.animatedSprite.Animation != "kick") return;
            stateMachine.TransitionTo("IdleMovementState");
        }

        private void SetHitBoxActive(bool active)
        {
            if (_attackHitBox == null) return;

            _attackHitBox.Monitoring = active;
            foreach (var child in _attackHitBox.GetChildren())
            {
                if (child is CollisionShape2D shape)
                {
                    shape.SetDeferred(CollisionShape2D.PropertyName.Disabled, !active);
                }
            }
        }
    }
}