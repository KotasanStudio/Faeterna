using Godot;
using Faeterna.Scripts.Personaje;
using EnemyClass = Faeterna.Scripts.Enemigos.Enemy.Enemy;

namespace Faeterna.Scripts.Enemigos.Slime
{
    public partial class Slime : EnemyClass
    {
        /// <summary>>Fuerza vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
        public float JumpForce = -380f;
        /// <summary>Intervalo de tiempo entre cada salto (en segundos).</summary>
        public float JumpInterval = 2.0f;
        public float HorizontalSpeed = 120f;

        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        public int Health = 6;
        private Timer _jumpTimer;
        private bool _isJumping = false;
        private int _jumpDirection = 1; // Alterna entre -1 y 1

        private Node2D _target = null; // Si quieres perseguir al jugador
        private float _knockbackTimer = 0f;
        private const float KnockbackDuration = 0.2f;
        private ShaderMaterial _shaderMaterial;

        [Export] private CollisionShape2D _detectionArea;
        [Export] private Area2D _hurtBox;

        public override void _Ready()
        {

            
            _jumpTimer = new Timer
            {
                WaitTime = JumpInterval,
                OneShot = false
            };
            _jumpTimer.Timeout += OnJumpTimerTimeout;
            AddChild(_jumpTimer);
            _jumpTimer.Start();
            _shaderMaterial = (ShaderMaterial)_animatedSprite.Material;
            SetAnimation("idle");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 velocity = Velocity;

            if (!IsOnFloor())
                velocity.Y += Gravity * (float)delta;

            if (_isJumping && IsOnFloor() && velocity.Y >= 0)
            {
                _isJumping = false;
                velocity.X = 0;
                velocity.Y = 0;
                SetAnimation("idle");
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

        private void OnJumpTimerTimeout()
        {
            if (!IsOnFloor()) return;

            _isJumping = true;

            float directionX;

            if (_target != null)
            {
                // Saltar hacia el jugador si está detectado
                directionX = Mathf.Sign(_target.GlobalPosition.X - GlobalPosition.X);
            }
            else
            {
                // Alternar de lado a lado
                directionX = _jumpDirection;
                _jumpDirection *= -1;
            }

            // Voltear sprite según dirección
            _animatedSprite.FlipH = directionX < 0;
            _detectionArea.Position = new Vector2(88.25f * directionX, 0); // Ajusta el área de detección

            Velocity = new Vector2(directionX * HorizontalSpeed, JumpForce);
            SetAnimation("jump");
        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                _target = lira; // Empieza a perseguir al jugador
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
                _target = null; // Vuelve a alternar lados
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
            GD.Print("Slime take damage: " + v + ", remaining health: " + Health);
            if (Health <= 0)
            {
                QueueFree();
                return;
            }
            _isJumping = false;
            _knockbackTimer = KnockbackDuration;

            // Dirección opuesta al atacante + mini salto
            float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
            Velocity = new Vector2(directionX * 250f, -200f);
        }
    }
}
