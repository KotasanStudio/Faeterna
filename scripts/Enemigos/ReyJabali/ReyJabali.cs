using System;
using Faeterna.Scripts.Enemigos.Enemy;
using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.scripts.Enemigos.ReyJabali
{
    public partial class ReyJabali : Enemy
    {
        public float DashSpeed = 300f;
        private float _dashDuration = 2f;
        public int Health = 8;
        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        private ShaderMaterial _shaderMaterial;

        // <summary>Indica si el enemigo está actualmente en estado de dash.</summary>
        private bool _isDashing = false;
        private bool _isChargingAttack = false;
        private bool _isWall = true;
        // <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>

        private int _dashDirection = 1;
        private Node2D _target = null;
        private float _knockbackTimer = 0f;
        private const float KnockbackDuration = 0.2f;

        private enum enemyDirection
        {
            Left = -1,
            Right = 1
        }
        [Export] private enemyDirection _currentDirection = enemyDirection.Right;
        [Export] private CollisionShape2D _detectionArea;
        [Export] private Area2D _attackHitBox;
        [Export] private RayCast2D _groundCheck;
        [Export] private Area2D _hurtBox;
        [Export] private Timer _loadAttackTimer;
        [Export] private Timer _deathAnimationTimer;
        [Export] private Timer _dashTimer;

        private bool _isDead = false;
        private Random _rnd = new();
        public override void _Ready()
        {
            flipHJabali(_currentDirection == enemyDirection.Right ? 1 : -1);  
            //_dashTimer.Start();
            _shaderMaterial = (ShaderMaterial)_animatedSprite.Material;
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


                flipHJabali(_dashDirection);
                            
            if (_isChargingAttack)
            {
                velocity.X = 0;
                SetAnimation("loadAttack");
            }
            else if (_isDashing)
            {
                velocity.X = _dashDirection * DashSpeed;

                SetAnimation("run");

                _groundCheck.ForceRaycastUpdate();

                if (!_groundCheck.IsColliding())
                {
                    StopDash(ref velocity);
                }
            }
            else
            {
                velocity.X = 0;
                SetAnimation("idle");
            }
            }

            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= (float)delta;
                if (_knockbackTimer <= 0f)
                {
                    // Termina el estado de knockback
                    _knockbackTimer = 0f;
                    velocity.X = 0f; // Detiene el movimiento horizontal después del knockback
                }
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private async void OnDashTimerTimeout()
        {
            if (!IsOnFloor())
                return;

            _isDashing = true;

            await ToSignal(
                GetTree().CreateTimer(_dashDuration),
                "timeout"
            );

            if(IsOnWall()){
            float directionX = GlobalPosition.X * -1;
            Velocity = new Vector2(directionX * 250f, -200f);
                _dashDirection *=-1;
                flipHJabali(_dashDirection);
                
            }else{
                Random _rng = new Random(); 
                _dashDirection = (int)(_rng.Next(0, 2) == 0
                ? enemyDirection.Left
                : enemyDirection.Right);
            }
            _isDashing = false;
   
        }
        public void _on_load_attack_timer_timeout()
        {
            if (_target == null || !IsOnFloor())
                return;

            _isChargingAttack = false;
            _isDashing = true;

            float directionX =
                Mathf.Sign(_target.GlobalPosition.X - GlobalPosition.X);

            if (directionX == 0)
                directionX = 1;

            _dashDirection = (int)directionX;

            flipHJabali(directionX);
        }

        private void StopDash(ref Vector2 velocity)
        {
            _isDashing = false;

            velocity.X = 0;

            SetAnimation("idle");

            _dashTimer.Start();
        }


        private void flipHJabali(float directionX)
        {
            _animatedSprite.FlipH = directionX < 0;
            _groundCheck.Position = new Vector2(Mathf.Abs(_groundCheck.Position.X) * directionX, _groundCheck.Position.Y); // Ajusta la posición del raycast según la dirección
            _detectionArea.Position = new Vector2(156.25f * directionX, 0); // Ajusta el área de detección
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
                TakeDamage((int)(shot.Scale.X*1.5f), shot.GlobalPosition);
        }
        private void TakeDamage(int v, Vector2 globalPosition)
        {
            Health -= v;
            hitShader(_shaderMaterial);
            if(_target != null)
                flipHJabali(GlobalPosition.X * -1);
            if (Health <= 0)
            {
                desactiveCollision();
                _isDead = true;
                Velocity = Vector2.Zero;
                _isDashing = false;
                _isChargingAttack = false;
                _dashTimer.Stop();
                _loadAttackTimer.Stop();
                SetAnimation("die");
                // Espera a que termine la animación de muerte antes de eliminar
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

        private void desactiveCollision()
        {
            _attackHitBox.CollisionLayer = 0;
            _attackHitBox.CollisionMask = 0;
            _detectionArea.GetParent<Area2D>().CollisionLayer = 0;
            _detectionArea.GetParent<Area2D>().CollisionMask = 0;
        }

        public void _on_detection_area_body_entered(Node2D prota)
        {

            
            if (prota is Lira lira)
            {
                Velocity = Vector2.Zero; // Detiene el movimiento al detectar al jugador
                _target = lira; // Empieza a perseguir al jugador
                _dashTimer.Stop(); // Detiene el timer de dash automático
                _isDashing = false; // Fuerza la salida del estado de dash
                Velocity = Vector2.Zero; // Detiene el movimiento al detectar al jugador
                _isChargingAttack = true; // Asegura que no esté en estado de carga al detectar al jugador
                SetAnimation("loadAttack"); // Comienza la animación de carga
                _loadAttackTimer.Start(); // Inicia el timer para cargar el ataque
            }
            
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
            {
                if (!_isDead) // Solo reanuda el timer si no está muerto
                    _dashTimer.Start(); // Reanuda el timer de dash automático
            }
        }


    }
}
