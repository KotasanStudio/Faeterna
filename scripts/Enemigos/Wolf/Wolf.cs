using System;
using Faeterna.Scripts.Personaje;
using EnemyClass = Faeterna.Scripts.Enemigos.Enemy.Enemy;
using Faeterna.Scripts.Enemigos.Enemy;
using Godot;

namespace Faeterna.scripts.Enemigos.Wolf
{
    public partial class Wolf : EnemyClass
    {
        public float DashSpeed = 300f;
        public float DashInterval = 2f;
        // <summary>Duración del impulso de dash en segundos.</summary>
        public float DashDuration = 1f;
        public int Health = 5;
        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        // <summary>Indica si el enemigo está actualmente en estado de dash.</summary>
        private Timer _dashTimer;
        private bool _isDashing = false;
        // <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>
        private float _dashDuration = 0.5f;
        private int _dashDirection = 1;
        private Node2D _target = null;
        private float _knockbackTimer = 0f;
        private const float KnockbackDuration = 0.2f;
        private ShaderMaterial _shaderMaterial;
        [Export] private CollisionShape2D _detectionArea;
        [Export] private RayCast2D _groundCheck;
        [Export] private Area2D _hurtBox;
        private Random _rnd = new();
        public override void _Ready()
        {
            _dashTimer = new Timer
            {
                WaitTime = DashInterval,
                OneShot = false
            };
            _dashTimer.Timeout += OnDashTimerTimeout;
            AddChild(_dashTimer);
            _dashTimer.Start();
            _shaderMaterial = (ShaderMaterial)_animatedSprite.Material;
            SetAnimation("idle");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 velocity = Velocity;

            if (!IsOnFloor())
                velocity.Y += Gravity * (float)delta;

            if (_isDashing)
            {
                // Durante el dash, mantenemos la velocidad constante en la dirección del dash.
                _dashDuration -= (float)delta;

                // Fuerza la actualización del raycast en cada frame
                _groundCheck.ForceRaycastUpdate();

                // Para el dash si no hay suelo delante o se acabó el tiempo límite
                if ((!_groundCheck.IsColliding() && _target == null) || _dashTimer.WaitTime <= 0f)
                {
                    _isDashing = false;
                    velocity.X = 0;
                    SetAnimation("idle");
                }
            }

        if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= (float)delta;
                if (IsOnFloor())
                {
                    velocity.X = 0f;
                }
            }

            Velocity = velocity;
            MoveAndSlide();
        }
        private void OnDashTimerTimeout()
        {
            if (!IsOnFloor()) return;

            _isDashing = true;
            _dashDuration = _rnd.Next((int)(DashDuration * 0.75f), (int)(DashDuration * 1.25f)); // Añade algo de variación a la duración del dash

            float directionX;

            if (_target != null)
                directionX = Mathf.Sign(_target.GlobalPosition.X - GlobalPosition.X);
            else
            {
                directionX = _dashDirection;
                _dashDirection *= -1;
            }

            _animatedSprite.FlipH = directionX < 0;
            _groundCheck.Position = new Vector2(Mathf.Abs(_groundCheck.Position.X) * directionX, _groundCheck.Position.Y); // Ajusta la posición del raycast según la dirección
            _detectionArea.Position = new Vector2(156.25f * directionX, 0); // Ajusta el área de detección
            Velocity = new Vector2(directionX * DashSpeed, Velocity.Y);
            SetAnimation("run");
        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }
        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (area.GetParent() is Lira lira)
                TakeDamage(1, lira.GlobalPosition);
             if (area is Shot shot)
                TakeDamage((int)shot.Scale.X, shot.GlobalPosition);
        }
        private void TakeDamage(int v, Vector2 globalPosition)
        {
            Health -= v;
                hitShader(_shaderMaterial);
            if (Health <= 0)
            {
                QueueFree();
                return;
            }
            _isDashing = false;
            _knockbackTimer = KnockbackDuration;

            // Dirección opuesta al atacante + mini salto
            float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
            Velocity = new Vector2(directionX * 250f, -200f);
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
            {
                
                _target = lira;
            }
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                _target = null;
        }


    }
}
