using System;
using System.ComponentModel;
using Faeterna.Scripts.Enemigos;
using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.scripts.Enemigos.ReyJabali
{
    public partial class ReyJabali : Enemy
    {
        // ── Parámetros de movimiento ──────────────────────────────────────────
        private ShaderMaterial _shaderMaterial;

        // ── Estado ────────────────────────────────────────────────────────────
        private bool _isDashing = false;
        private bool _isChargingAttack = false;
        private bool _isDead = false;

        private bool _jumped = false;
        private bool _falled = false;

        private int _dashCount = 0;
        private int _jumpCount = 0;
        private int _actionsBeforeJump = 3;
        private bool _isReturningHome = false;
        public float WalkSpeed = 150f; // Más lento que el Dash
        private float _arrivalThreshold = 10f; // Distancia mínima para considerar que llegó

        // ── Dirección ─────────────────────────────────────────────────────────
        private int _currentDirection;
        private Vector2 _initPosition; // Guardamos la posición inicial para posibles resets
        [Export] private bool _startFacingLeft;

        [ExportGroup("Collisions and Areas")]
        [Export] private Area2D _bossArea; // Área de detección del jugador;
        [Export] private Area2D _attackHitBox;
        [Export] private Area2D _hurtBox;
        
        [ExportGroup("Timers")]
        [Export] private Timer _dashTimer; // Timer para controlar la duración del dash
        [Export] private Timer _loadAttackTimer; // Timer para controlar la carga del ataque
        [Export] private Timer _deathAnimationTimer; // Timer para controlar la duración de la animación de muerte

        [ExportGroup("Audio")]
        [Export] private AudioStream _jumpAudio;
        [Export] private AudioStream _hitAudio;
        [Export] private AudioStream _runAudio;


        [Signal] public delegate void jabaliBossdeathEventHandler();
        private Random _rnd = new();


        public override void _EnterTree()
        {
            _initPosition = GlobalPosition; // Guardamos la posición inicial para posibles resets
        }
        public override void _Ready()
        {
            if (_startFacingLeft)
            {
                _currentDirection = 1;
                FlipHJabali();
            }
            else
                _currentDirection = -1;
            _shaderMaterial = (ShaderMaterial)animatedSprite.Material;
            SetAnimation("idle");

        }

        // ─────────────────────────────────────────────────────────────────────
        // PHYSICS PROCESS
        // ─────────────────────────────────────────────────────────────────────
        public override void _PhysicsProcess(double delta)
        {
            if (_isDead)
                return;

            Vector2 velocity = Velocity;

            if (!IsOnFloor())
            {
                velocity.Y += Gravity * (float)delta;
                if (velocity.Y < 0 && !_jumped)
                {
                    SetAnimation("jump");
                    playAudio(_jumpAudio);
                    _jumped = true;
                    _falled = false;
                }
                else if (velocity.Y > 0 && !_falled)
                {
                    SetAnimation("fall");
                    _falled = true;
                    _jumped = false;
                }
            }
            else
            {
                // PRIORIDAD 1: Si el jugador está cerca, atacar
                if (target != null)
                {
                    _isReturningHome = false; // Cancelamos el regreso si el jugador vuelve
                    if (_isChargingAttack)
                    {
                        _dashTimer.Stop(); // Asegura que el dash se detenga si estaba activo
                        SetAnimation("loadAttack");
                    }
                    else if (_isDashing)
                    {
                        velocity.X = _currentDirection * dashSpeed;
                        SetAnimation("run");
                    }
                    else if (!_isChargingAttack && !_isDashing)
                    {
                        bool shouldFlip = (target.GlobalPosition.X > GlobalPosition.X && _currentDirection < 0) ||
                            (target.GlobalPosition.X < GlobalPosition.X && _currentDirection > 0);
                        if (shouldFlip)
                            FlipHJabali();


                        _isChargingAttack = true;
                        SetAnimation("idle");
                        playAudio(null);

                    }
                }
                // PRIORIDAD 2: Si el jugador se fue, caminar a casa
                else if (_isReturningHome)
                {
                    float distanceToStart = _initPosition.X - GlobalPosition.X;

                    if (Mathf.Abs(distanceToStart) > _arrivalThreshold)
                    {
                        // Calcular dirección hacia el punto inicial
                        int directionHome = distanceToStart > 0 ? 1 : -1;

                        if (directionHome != _currentDirection)
                            FlipHJabali();

                        velocity.X = directionHome * WalkSpeed;
                        SetAnimation("run");

                    }
                    else
                    {
                        // LLEGÓ A CASA
                        velocity.X = 0;
                        _isReturningHome = false;
                        SetAnimation("idle");
                        int initialDir = _startFacingLeft ? -1 : 1;
                        if (_currentDirection != initialDir)
                            FlipHJabali();
                    }
                }
                // PRIORIDAD 3: Quieto
                else
                {
                    velocity.X = Mathf.MoveToward(Velocity.X, 0, dashSpeed * (float)delta);
                    if (velocity.X == 0)
                        SetAnimation("idle");
                }
            }

            Velocity = velocity;
            MoveAndSlide();
        }
        // ─────────────────────────────────────────────────────────────────────
        // CARGAR ATAQUE (Timer)
        // ─────────────────────────────────────────────────────────────────────
        public void _on_load_attack_timer_timeout()
        {
            if (target == null || _isDead)
                return;

            _isChargingAttack = false;
            bool shouldFlip = (target.GlobalPosition.X > GlobalPosition.X && _currentDirection < 0) ||
                              (target.GlobalPosition.X < GlobalPosition.X && _currentDirection > 0);
            if (shouldFlip)
                FlipHJabali();

            if (_dashCount >= _actionsBeforeJump)
            {
                Velocity = DoNextJump();

                GD.Print($"Salto {_jumpCount} ejecutado");

                if (_jumpCount < 3)
                {
                    _loadAttackTimer.Start(0.8f);
                }
                else
                {
                    // Reiniciar ciclo completo
                    _jumpCount = 0;
                    _dashCount = 0;
                    _actionsBeforeJump = _rnd.Next(2, 5);

                    _isChargingAttack = true;

                    // Espera antes de volver al patrón normal
                    _loadAttackTimer.Start(2f);
                }
            }
            else
            {
                // FASE DE CARRERA
                _dashCount++;
                playAudio(_runAudio);
                _isDashing = true; // Activamos el estado de dash para que el _PhysicsProcess lo gestione
                _dashTimer.Start(); // Inicia el timer para controlar la duración del dash

            }
        }

        public void _on_dash_timer_timeout()
        {
            _isDashing = false;
            _isChargingAttack = true;

            if (target != null)
            {
                int directionToPlayer = target.GlobalPosition.X > GlobalPosition.X ? 1 : -1;
                if (directionToPlayer != _currentDirection)
                    FlipHJabali();
            }

            _loadAttackTimer.Start();
        }
        // ─────────────────────────────────────────────────────────────────────
        // ATAQUE ESPECIAL — un salto a la vez
        // ─────────────────────────────────────────────────────────────────────
        private Vector2 DoNextJump()
        {
            if (_jumpCount >= 3)
            {
                _jumpCount = 0;
                _dashCount = 0;
                _actionsBeforeJump = _rnd.Next(1, 3);  // randomiza para el siguiente ciclo

                GD.Print("Ataque especial completado.");
                return new Vector2(0, Velocity.Y);
                // El reinicio del ciclo lo maneja _on_load_attack_timer_timeout
            }

            _jumpCount++;
            return new Vector2(_currentDirection * (dashSpeed/2), jumpVelocity);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FLIP
        // ─────────────────────────────────────────────────────────────────────
        private void FlipHJabali()
        {
            _currentDirection *= -1;
            animatedSprite.FlipH = _currentDirection < 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DAÑO
        // ─────────────────────────────────────────────────────────────────────
        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (_isDead)
                return;

            if (area.Name == "KickHitbox" && area.GetParent() is Lira lira)
            {
                TakeDamage(1, lira.GlobalPosition);

            }
            else if (area is Shot shot)
            {
                TakeDamage((int)(shot.Scale.X * 1.5f), shot.GlobalPosition);

            }
            if (target != null)
            {
                bool shouldFlip = (target.GlobalPosition.X > GlobalPosition.X && _currentDirection < 0) ||
                                  (target.GlobalPosition.X < GlobalPosition.X && _currentDirection > 0);
                if (shouldFlip)
                    FlipHJabali();
            }


        }

        private void TakeDamage(int amount, Vector2 sourcePosition)
        {
            health -= amount;
            hitShader(_shaderMaterial);
            playAudio(_hitAudio);
            if (health > 0)
                return;

            _isDead = true;
            _isDashing = false;
            _isChargingAttack = false;
            Velocity = Vector2.Zero;

            _loadAttackTimer.Stop();

            DesactiveCollision();
            SetAnimation("die");
            EmitSignal(nameof(jabaliBossdeath));
            _deathAnimationTimer.Start();
        }

        private void OnDeathAnimationTimerTimeout() => QueueFree();

        // ─────────────────────────────────────────────────────────────────────
        // COLISIONES
        // ─────────────────────────────────────────────────────────────────────
        private void DesactiveCollision()
        {
            _attackHitBox.CollisionLayer = 0;
            _attackHitBox.CollisionMask = 0;

            _bossArea.CollisionLayer = 0;
            _bossArea.CollisionMask = 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DETECCIÓN DEL JUGADOR
        // ─────────────────────────────────────────────────────────────────────
        public void _on_boss_area_body_entered(Node2D prota)
        {
            GD.Print("Rey Jabali detecta algo entrando al área");
            if (prota is not Lira lira)
                return;

            target = lira;
            _isReturningHome = false; // Deja de caminar a casa para pelear
            _isChargingAttack = true;
            _isDashing = false;

            // Girar hacia el jugador al entrar
            int newDir = lira.GlobalPosition.X > GlobalPosition.X ? 1 : -1;
            if (newDir != _currentDirection)
                FlipHJabali();

            _loadAttackTimer.Start();
        }

        public void _on_boss_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
            {
                target = null;
                _isDashing = false;
                _isChargingAttack = false;
                _loadAttackTimer.Stop();

                // Esperar un segundo antes de decidir volver (por si el jugador entra de nuevo)
                GetTree().CreateTimer(1.0f).Timeout += () =>
                {
                    if (target == null)
                        _isReturningHome = true;
                };
            }
        }

        private void moveToinitPosition()
        {
            if (target != null || _isDead)
                return; // No volver si el jugador regresó
            GlobalPosition = _initPosition;
            Velocity = Vector2.Zero;

            // Ajustar dirección inicial
            int initialDir = _startFacingLeft ? -1 : 1;
            if (_currentDirection != initialDir)
                FlipHJabali();

            _isChargingAttack = false;
            _isDashing = false;
            SetAnimation("idle");
        }
    }
}