using System;

using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.scripts.Enemigos.Jabali
{
    public partial class Jabali : CharacterBody2D
    {
        public float DashSpeed = 300f;
        public float DashInterval = 2f;
        // <summary>Duración del impulso de dash en segundos.</summary>
        public float DashDuration = 1f;
        public int Health = 8;
        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        private AnimatedSprite2D _animatedSprite;
        // <summary>Indica si el enemigo está actualmente en estado de dash.</summary>
        private Timer _dashTimer;
        private bool _isDashing = false;
        private bool _isChargingAttack = false;
        // <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>
        private float _dashDuration = 0.5f;
        private int _dashDirection = 1;
        private Node2D _target = null;
        private float _knockbackTimer = 0f;
        private const float KnockbackDuration = 0.2f;
        [Export] private CollisionShape2D _detectionArea;
        [Export] private RayCast2D _groundCheck;
        [Export] private Area2D _hurtBox;
        [Export] private Timer _loadAttackTimer;
        private Timer _deathAnimationTimer;
        private bool _isDead = false;
        private Random _rnd = new();
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
             _dashTimer = new Timer
            {
                WaitTime = DashInterval,
                OneShot = false
            };
            _dashTimer.Timeout += OnDashTimerTimeout;
            AddChild(_dashTimer);

            _dashTimer.Start();
            
            _deathAnimationTimer = new Timer
            {
                OneShot = true
            };
            _deathAnimationTimer.Timeout += OnDeathAnimationTimerTimeout;
            AddChild(_deathAnimationTimer);
            
            SetAnimation("idle");
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isDead)
                return;
                
            Vector2 velocity = Velocity;

            if (!IsOnFloor())
                velocity.Y += Gravity * (float)delta;
            else
            {
                if (_isChargingAttack)
                {
                    // Mientras carga el ataque, detiene completamente el movimiento
                    velocity.X = 0;
                }
                else if (_isDashing)
                {
                    // Durante el dash, mantiene la velocidad hacia la dirección del ataque
                    // Fuerza la actualización del raycast en cada frame
                    _groundCheck.ForceRaycastUpdate();

                    // Para el dash si choca con una pared o el objetivo se perdió
                    if (!_groundCheck.IsColliding() && _target == null)
                    {
                        _isDashing = false;
                        velocity.X = 0;
                        SetAnimation("idle");
                    }
                }

                if (velocity.X == 0f && !_isChargingAttack)
                {
                    SetAnimation("idle");
                }
                if (velocity.X == 0f && _isChargingAttack)
                {
                    SetAnimation("loadAttack");
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

                directionX = _dashDirection;
                _dashDirection *= -1;

            _animatedSprite.FlipH = directionX < 0;
            _groundCheck.Position = new Vector2(Mathf.Abs(_groundCheck.Position.X) * directionX, _groundCheck.Position.Y); // Ajusta la posición del raycast según la dirección
            _detectionArea.Position = new Vector2(156.25f * directionX, 0); // Ajusta el área de detección
            Velocity = new Vector2(directionX * DashSpeed, Velocity.Y);
            SetAnimation("run");
        }


        public void SetAnimation(string animationName)
        {
            _animatedSprite?.Play(animationName);
        }

        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }
        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (_isDead) return;
            if (area.GetParent() is Lira lira)
                TakeDamage(1, lira.GlobalPosition);
             if (area is Shot shot)
                TakeDamage((int)shot.Scale.X, shot.GlobalPosition);
        }
        private void TakeDamage(int v, Vector2 globalPosition)
        {
            SetAnimation("hit");
            Health -= v;
            // Dirección opuesta al atacante + mini salto
            
            float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
            Velocity = new Vector2(directionX * 250f, -200f);
            
            
            if (Health <= 0)
            {
                _isDead = true;
                Velocity = Vector2.Zero;
                _isDashing = false;
                _isChargingAttack = false;
                _dashTimer.Stop();
                _loadAttackTimer.Stop();
                SetAnimation("die");
                // Espera a que termine la animación de muerte antes de eliminar
                _deathAnimationTimer.WaitTime = 2.5f; // Ajusta este valor al tiempo de tu animación de muerte
                _deathAnimationTimer.Start();
                return;
            }
            _isDashing = false;
            _knockbackTimer = KnockbackDuration;

        }
        
        private void OnDeathAnimationTimerTimeout()
        {
            QueueFree();
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
            {
                _target = lira; // Empieza a perseguir al jugador
                _isChargingAttack = true; // Comienza a cargar el ataque
                _isDashing = false; // Detiene el dash actual si está en curso
                _dashTimer.Stop(); // Detiene el timer de dash automático
                Velocity = Godot.Vector2.Zero; // Detiene el movimiento
                SetAnimation("loadAttack"); // Comienza la animación de carga
                _loadAttackTimer.Start(); // Inicia el timer para cargar el ataque
            }
            
        }

        public void _on_load_attack_timer_timeout()
        {
            // Cuando el timer termina, ejecuta el ataque hacia el objetivo
            if (_target != null && IsOnFloor())
            {
                _isChargingAttack = false;
                _isDashing = true;
                
                // Calcula la dirección hacia el objetivo
                float directionX = Mathf.Sign(_target.GlobalPosition.X - GlobalPosition.X);
                _dashDirection = (int)directionX;

                _animatedSprite.FlipH = directionX < 0;
                _groundCheck.Position = new Vector2(Mathf.Abs(_groundCheck.Position.X) * directionX, _groundCheck.Position.Y); // Ajusta la posición del raycast según la dirección
                _detectionArea.Position = new Vector2(156.25f * directionX, 0); // Ajusta el área de detección
                Velocity = new Vector2(directionX * DashSpeed, Velocity.Y);
                SetAnimation("run");
            }
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
            {
                _target = null;
                _isChargingAttack = false;
                if (!_isDead) // Solo reanuda el timer si no está muerto
                    _dashTimer.Start(); // Reanuda el timer de dash automático
            }
        }


    }
}
