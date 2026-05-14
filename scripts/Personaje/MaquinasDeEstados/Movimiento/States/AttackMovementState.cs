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
        private AnimationPlayer _animPlayer;
        private bool _animationConnected = false;
        private bool _frameConnected = false;

        public override void Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("Lira");
            if (_player == null) return;

            _attackHitBox = _player.GetNodeOrNull<Area2D>("KickHitbox");
            _animPlayer   = _player.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

            SetHitBoxActive(false);
            // FrameChanged se suscribe en Enter() para evitar que se llame
            // antes de que animatedSprite esté listo
        }

        public override void Enter()
        {
            if (_player == null) return;

            _player.SetAnimation("kick");

            if (!_frameConnected)
            {
                _player.animatedSprite.FrameChanged += OnFrameChanged;
                _frameConnected = true;
            }

            if (_animPlayer != null && !_animationConnected)
            {
                _animPlayer.AnimationFinished += OnAnimationFinished;
                _animationConnected = true;
            }
        }

        public override void Exit()
        {
            SetHitBoxActive(false);

            if (_player?.animatedSprite != null && _frameConnected)
            {
                _player.animatedSprite.FrameChanged -= OnFrameChanged;
                _frameConnected = false;
            }

            if (_animPlayer != null && _animationConnected)
            {
                _animPlayer.AnimationFinished -= OnAnimationFinished;
                _animationConnected = false;
            }
        }

        public override void HandleInput(InputEvent ev) { }
        public override void Update(double delta) { }

        private void OnFrameChanged()
        {
            // Solo activa el hitbox si estamos en el estado de ataque
            if (stateMachine?.CurrentStateName != "AttackMovementState") return;
            if (_player.animatedSprite.Animation != "kick") return;

            int frame = _player.animatedSprite.Frame;
            SetHitBoxActive(frame == 2 || frame == 3);
        }

        private void OnAnimationFinished(StringName animName)
        {
            // Si usas AnimationTree, a veces el animName viene con la ruta del árbol
            // Por eso es mejor verificar si contiene la palabra "kick"
            if (animName.ToString().Contains("kick"))
            {
                GD.Print("Ataque finalizado, volviendo a Idle");
                stateMachine.TransitionTo("IdleMovementState");
            }
        }

        private void SetHitBoxActive(bool active)
        {
            if (_attackHitBox == null) return;
            _attackHitBox.Monitoring = active;
            foreach (var child in _attackHitBox.GetChildren())
            {
                if (child is CollisionShape2D shape)
                    shape.SetDeferred(CollisionShape2D.PropertyName.Disabled, !active);
            }
        }
        public void FinalizarAtaque()
        {
            if (stateMachine?.CurrentStateName == "AttackMovementState")
            {
                stateMachine.TransitionTo("IdleMovementState");
            }
        }
    }
}