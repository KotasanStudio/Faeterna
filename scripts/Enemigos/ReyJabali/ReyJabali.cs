using System;
using Faeterna.Scripts.Enemigos.Enemy;
using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.scripts.Enemigos.ReyJabali
{
    public partial class ReyJabali : Enemy
    {
        public float DashSpeed = 400f;
        private float _jumpVelocity = -800f; 
        private float _dashDuration = 2f;
        public int Health = 80;
        private static float Gravity => ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        private ShaderMaterial _shaderMaterial;

        // <summary>Indica si el enemigo está actualmente en estado de dash.</summary>
        private bool _isDashing = false;
        private bool _isChargingAttack = false;
        private bool _isWall = true;
        // <summary>Timer que controla la duración del dash (cuánto tiempo dura el impulso).</summary>

        private int _dashDirection = 1;
        private int _dashCount = 0; // Contador de dashes en el ataque especial
        private int _jumpCount = 0; // Contador de saltos para el ataque especial
        private bool _specialAttackInProgress = false; // Indica si el ataque especial está en progreso
        private Node2D _target = null;

        private enum enemyDirection
        {
            Left = -1,
            Right = 1
        }
        [Export] private enemyDirection _currentDirection;
        [Export] private CollisionShape2D _detectionArea;
        [Export] private Area2D _attackHitBox;
        [Export] private Area2D _hurtBox;
        [Export] private Timer _loadAttackTimer;
        [Export] private Timer _deathAnimationTimer;
        [Export] private Timer _dashTimer;

        private bool _isDead = false;
        private Random _rnd = new();
        public override void _Ready()
        {
                flipHJabali((int)_currentDirection);  
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
            if(_dashCount==3 || _specialAttackInProgress)
                {
                    _isDashing = false;
                    _isChargingAttack = false;
                    _specialAttackInProgress = true; // Marca que el ataque especial está en progreso
                    if(_target != null)
                    velocity = specialAttack();
                    if(velocity.Y > 0)
                        SetAnimation("fall");
                    else if(velocity.Y < 0)
                        SetAnimation("jump");
                    else
                    {
                        flipHJabali((int)_currentDirection * -1); // Voltea en la dirección opuesta si no hay objetivo
                    }
                }
            else if (_isChargingAttack)
            {
                velocity.X = 0;
                SetAnimation("loadAttack");
            }
            else if (_isDashing)
            {
                velocity.X = _dashDirection * DashSpeed;

                SetAnimation("run");

                if(Velocity.X == 0)
                {
                    flipHJabali(_dashDirection * -1);
                    StopDash(ref velocity);
                }
            }
            else
            {                    velocity = specialAttack();

                velocity.X = 0;
                SetAnimation("idle");
            }
            }


            Velocity = velocity;
            MoveAndSlide();
        }

        private async void OnDashTimerTimeout()
        {
            _dashCount++; // Incrementa el contador de dashes
            GD.Print("Dash timer timeout, attempting to dash..." + _dashCount);
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
                
            }
            _isDashing = false;
            
   
        }
        public void _on_load_attack_timer_timeout()
        {
            if (_target == null || !IsOnFloor())
                return;

            _isChargingAttack = false;
            _isDashing = true;

        }

        private  Vector2 specialAttack()
        {
            SetAnimation("loadAttack");
            // Ataque especial: 3 dashes consecutivos hacia el jugador
            _isDashing = false;
            _isChargingAttack = false;
            _dashTimer.Stop(); // Detiene el timer de dash automático para evitar que interfiera con el ataque especial
            _loadAttackTimer.Stop(); // Detiene el timer de carga para evitar que interfiera con el ataque especial
            
                float jumpDirectionX = Mathf.Sign(_target.GlobalPosition.X - GlobalPosition.X); 
                GD.Print("Jump direction X: " + jumpDirectionX);               
                flipHJabali(jumpDirectionX);
                
                Vector2 velocity = Velocity;
                velocity.X = jumpDirectionX * DashSpeed;
                velocity.Y = _jumpVelocity;
            
            _dashCount--; // Reinicia el contador después del ataque especial
            _isDashing = false;
            _jumpCount++;

            if(_jumpCount == 3){
                _specialAttackInProgress = false; // Marca que el ataque especial ha terminado
                _jumpCount = 0; // Reinicia el contador de saltos para el próximo ataque especial
                _dashCount = 0; // Reinicia el contador de dashes para el próximo ataque especial
                }
            return velocity;

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
            if (area.GetParent() is Lira lira){
                TakeDamage(1, lira.GlobalPosition);
                if (lira.GlobalPosition.X > GlobalPosition.X)
                    flipHJabali(1); // Voltea hacia la derecha
                else
                    flipHJabali(-1); // Voltea hacia la izquierda
                }
            if (area is Shot shot){
                TakeDamage((int)(shot.Scale.X*1.5f), shot.GlobalPosition);
                if (shot.GlobalPosition.X > GlobalPosition.X)
                    flipHJabali(1); // Voltea hacia la derecha
                else
                    flipHJabali(-1); // Voltea hacia la izquierda
                    }
        }
        private void TakeDamage(int v, Vector2 globalPosition)
        {
            Health -= v;
            hitShader(_shaderMaterial);
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
            
            if(_specialAttackInProgress) return; // Ignora nuevas detecciones si el ataque especial ya está  en progreso
            if(_isDashing) return; // Ignora nuevas detecciones si ya está en estado de dash
            if(_isChargingAttack) return; // Ignora nuevas detecciones si ya está cargando un ataque
            
            if (prota is Lira lira)
            {
                Velocity = Vector2.Zero; // Detiene el movimiento al detectar al jugador
                _target = lira; // Empieza a perseguir al jugador
                _dashTimer.Stop(); // Detiene el timer de dash automático
                _isDashing = false; // Fuerza la salida del estado de dash
                Velocity = Vector2.Zero; // Detiene el movimiento al detectar al jugador
                _isChargingAttack = true; // Solo carga si no ha hecho 3 dashes
                SetAnimation("loadAttack"); // Comienza la animación de carga
                _loadAttackTimer.Start(); // Inicia el timer para cargar el ataque 
            }
            
        }

        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
            {
                _isChargingAttack = false; // Detiene la carga si el jugador sale del área de detección
                if (!_isDead) // Solo reanuda el timer si no está muerto
                    _dashTimer.Start(); // Reanuda el timer de dash automático
            }
        }


    }
}
