using System;
using Faeterna.Scripts.Enemigos;
using Faeterna.Scripts.Personaje;
using Godot;

namespace Faeterna.scripts.Enemigos.Jabali
{
    /// <summary>
    /// Jabalí enemigo con capacidad de ataque acelerado. Este enemigo alternamente
    /// corre de un lado a otro, y cuando detecta al jugador, carga un ataque antes
    /// de realizar un dash hacia él. Utiliza knockback y animaciones de carga.
    /// </summary>
    public partial class Jabali : Enemy
    {
        /// <summary>Duración del dash del jabalí en segundos.</summary>
        private float _dashDuration = 2f;

        /// <summary>Indica si el jabalí está actualmente en estado de dash/carrera.</summary>
        private bool _isDashing = false;

        /// <summary>Indica si el jabalí está cargando su ataque especial.</summary>
        private bool _isChargingAttack = false;

        /// <summary>Indica si el jabalí está contra una pared.</summary>
        private bool _isWall = true;

        /// <summary>Dirección actual del movimiento del jabalí (1 = derecha, -1 = izquierda).</summary>
        private int _dashDirection = 1;

        /// <summary>Referencia al objetivo actual del jabalí (el jugador si está detectado).</summary>
        private Node2D _target = null;

        /// <summary>Temporizador para controlar la duración del knockback.</summary>
        private float _knockbackTimer = 0f;

        /// <summary>Duración fija del knockback en segundos.</summary>
        private const float KnockbackDuration = 0.2f;

        /// <summary>Material de shader utilizado para efectos visuales de daño.</summary>
        private ShaderMaterial _shaderMaterial;

        /// <summary>Enumeración para las direcciones del jabalí.</summary>
        private enum enemyDirection
        {
            Left = -1,
            Right = 1
        }

        /// <summary>Dirección inicial del jabalí. Se asigna desde el editor.</summary>
        [Export] private enemyDirection _currentDirection = enemyDirection.Right;

        [ExportGroup("Timers")]
        /// <summary>Timer que controla la duración de la carga de ataque antes del dash. Se asigna desde el editor.</summary>
        [Export] private Timer _loadAttackTimer;

        /// <summary>Timer que espera a que la animación de muerte termine antes de eliminar el nodo. Se asigna desde el editor.</summary>
        [Export] private Timer _deathAnimationTimer;

        /// <summary>Timer que controla el intervalo de dashes automáticos cuando no hay objetivo. Se asigna desde el editor.</summary>
        [Export] private Timer _dashTimer;

        [ExportGroup("Audio")]
        /// <summary>Sonido de carrera del jabalí. Se asigna desde el editor.</summary>
        [Export] private AudioStream _runAudio;

        /// <summary>Sonido de impacto cuando el jabalí recibe daño. Se asigna desde el editor.</summary>
        [Export] private AudioStream _hitAudio;

        /// <summary>Indica si el jabalí está muerto.</summary>
        private bool _isDead = false;

        /// <summary>Generador de números aleatorios utilizado para determinar dirección aleatoria.</summary>
        private Random _rnd = new();

        /// <summary>
        /// Inicialización del jabalí. Configura la dirección inicial, inicia el timer de dash,
        /// obtiene el material del shader y establece la animación inicial a "idle".
        /// </summary>
        public override void _Ready()
        {

            flipHJabali(_currentDirection == enemyDirection.Right ? 1 : -1);
            _dashTimer.Start();
            _shaderMaterial = (ShaderMaterial)animatedSprite.Material;
            SetAnimation("idle");
        }

        /// <summary>
        /// Procesamiento de física cada frame. Gestiona la gravedad, el movimiento del dash,
        /// la carga de ataque, el knockback y el cambio de dirección.
        /// </summary>
        /// <param name="delta">Tiempo en segundos desde el último frame.</param>
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
                    velocity.X = _dashDirection * dashSpeed;

                    SetAnimation("run");
                    playAudio(_runAudio);
                    groundCheck.ForceRaycastUpdate();

                    if (!groundCheck.IsColliding() && target == null)
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
                    _knockbackTimer = 0f;
                    velocity.X = 0f;
                }
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        /// <summary>
        /// Maneja el timeout del timer de dash automático. Inicia el dash, espera su duración,
        /// y luego determina la siguiente dirección (aleatorio o rebote en pared).
        /// </summary>
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

        /// <summary>
        /// Maneja el timeout del timer de carga de ataque. Detiene la carga y comienza el dash
        /// hacia el objetivo detectado (jugador).
        /// </summary>
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

        /// <summary>
        /// Detiene el dash del jabalí, establece la velocidad a cero y reinicia el timer de dash automático.
        /// </summary>
        /// <param name="velocity">Velocidad actual del jabalí (se modifica por referencia).</param>
        private void StopDash(ref Vector2 velocity)
        {
            _isDashing = false;

            velocity.X = 0;

            SetAnimation("idle");

            _dashTimer.Start();
        }

        /// <summary>
        /// Voltea el sprite y ajusta la posición del raycast y área de detección según la dirección.
        /// </summary>
        /// <param name="directionX">Dirección horizontal (positiva = derecha, negativa = izquierda).</param>
        private void flipHJabali(float directionX)
        {
            animatedSprite.FlipH = directionX < 0;
            groundCheck.Position = new Vector2(Mathf.Abs(groundCheck.Position.X) * directionX, groundCheck.Position.Y); // Ajusta la posición del raycast según la dirección
            detectionArea.Position = new Vector2(156.25f * directionX, 0); // Ajusta el área de detección
        }

        /// <summary>
        /// Maneja la colisión del hitbox del ataque con otros cuerpos (el jugador recibe daño).
        /// </summary>
        /// <param name="prota">Nodo que colisionó con el hitbox.</param>
        public void _on_attack_hit_box_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
                lira.TakeDamage(1, GlobalPosition);
        }

        /// <summary>
        /// Maneja la colisión del hurtbox (área de daño recibido) con áreas de ataque.
        /// Aplica knockback y daño al jabalí si es golpeado por el jugador o un proyectil.
        /// </summary>
        /// <param name="area">Área que colisionó con el hurtbox.</param>
        private void OnHurtBoxAreaEntered(Area2D area)
        {
            if (_isDead) return;
            if (area.Name == "KickHitbox" && area.GetParent() is Lira lira){
                TakeDamage(1, lira.GlobalPosition);
                if (lira.GlobalPosition.X > GlobalPosition.X)
                {
                    flipHJabali(1); // Voltea hacia la derecha
                }
                else
                {
                    // Lira está a la izquierda, aplica knockback hacia la derecha
                    Velocity = new Vector2(250f, -200f);
                }}
            if (area is Shot shot)
                TakeDamage((int)(shot.Scale.X*1.5f), shot.GlobalPosition);
        }

        /// <summary>
        /// Reduce la salud del jabalí, aplica efectos visuales y de sonido, y maneja la muerte.
        /// </summary>
        /// <param name="v">Cantidad de daño a aplicar.</param>
        /// <param name="globalPosition">Posición del atacante, utilizada para determinar dirección del knockback.</param>
        private void TakeDamage(int v, Vector2 globalPosition)
        {
            health -= v;
            hitShader(_shaderMaterial);
            playAudio(_hitAudio);
            // Dirección opuesta al atacante + mini salto
            float directionX = GlobalPosition.X >= globalPosition.X ? 1.0f : -1.0f;
            Velocity = new Vector2(directionX * 250f, -200f);


            if (health <= 0)
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

        /// <summary>
        /// Maneja el timeout del timer de animación de muerte. Elimina el nodo del jabalí.
        /// </summary>
        private void OnDeathAnimationTimerTimeout()
        {
            QueueFree();
        }

        /// <summary>
        /// Desactiva las colisiones del jabalí para que no pueda golpear ni recibir daño después de muerto.
        /// </summary>
        private void desactiveCollision()
        {
            attackHitBox.CollisionLayer = 0;
            attackHitBox.CollisionMask = 0;
            detectionArea.GetParent<Area2D>().CollisionLayer = 0;
            detectionArea.GetParent<Area2D>().CollisionMask = 0;
        }

        /// <summary>
        /// Maneja la detección de un cuerpo dentro del área de detección (el jugador).
        /// Inicia la carga del ataque si el cuerpo detectado es el jugador.
        /// </summary>
        /// <param name="prota">Nodo que entró en el área de detección.</param>
        public void _on_detection_area_body_entered(Node2D prota)
        {
            if (prota is Lira lira)
            {
                Velocity = Vector2.Zero;
                _target = lira;
                _dashTimer.Stop();
                _isDashing = false;
                Velocity = Vector2.Zero;
                _isChargingAttack = true;
                SetAnimation("loadAttack");
                _loadAttackTimer.Start();
            }

        }

        /// <summary>
        /// Maneja la salida de un cuerpo del área de detección.
        /// Reanuda el movimiento automático si el jugador sale del rango.
        /// </summary>
        /// <param name="prota">Nodo que salió del área de detección.</param>
        public void _on_detection_area_body_exited(Node2D prota)
        {
            if (prota is Lira)
            {
                if (!_isDead)
                    _dashTimer.Start();
            }
        }
    }
}
